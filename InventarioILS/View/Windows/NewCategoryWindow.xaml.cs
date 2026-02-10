using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.Windows
{
    public partial class NewCategoryWindow : Window
    {
        readonly ItemSubcategories subcategories = ItemSubcategories.Instance;
        readonly ItemCategories categories = ItemCategories.Instance;

        public List<ItemMisc> SelectedSubcategories => [.. SubcategoryListBox.SelectedItems.Cast<ItemMisc>()];

        public NewCategoryWindow()
        {
            InitializeComponent();

            categories.Load();
            subcategories.Load();

            DataContext = new
            {
                SubcategoryList = subcategories.Items
            };
        }

        private async void CreateCategoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedIds = SelectedSubcategories.Select(subcat => subcat.Id).ToHashSet();
            var newCategory = new ItemMisc(CategoryNameInput.Text.ToLower(), CategoryShorthandInput.Text);

            using var transaction = DbConnection.CreateAndOpen().BeginTransaction();

            try
            {
                await CategoryService.RegisterCategoryAsync(newCategory, selectedIds, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                await StatusManager.Instance.UpdateMessageStatusAsync($"Error al crear categoría: {ex.Message}", StatusManager.MessageType.ERROR).ConfigureAwait(false);
                transaction.Rollback();
                return;
            }

            await categories.LoadAsync();
            Close();
        }

        private void CategoryNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = CategoryNameInput.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                CreateCategoryBtn.IsEnabled = false;
                CategoryNameErrorText.Visibility = Visibility.Collapsed;
                return;
            }

            bool exists = categories.Items.Any(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

            CategoryNameErrorText.Visibility = !exists ? Visibility.Collapsed : Visibility.Visible;
            if (exists) CategoryNameInput.Focus();

            CreateCategoryBtn.IsEnabled = !exists && !string.IsNullOrEmpty(CategoryShorthandInput.Text);
        }

        private void CategoryShorthandInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = CategoryShorthandInput.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                CreateCategoryBtn.IsEnabled = false;
                CategoryShorthandErrorText.Visibility = Visibility.Collapsed;
                return;
            }

            bool exists = categories.Items.Any(c => c.Shorthand.Equals(input, StringComparison.OrdinalIgnoreCase));

            CategoryShorthandErrorText.Visibility = !exists ? Visibility.Collapsed : Visibility.Visible;
            CreateCategoryBtn.IsEnabled = !exists && !string.IsNullOrEmpty(CategoryShorthandInput.Text);
        }
    }
}
