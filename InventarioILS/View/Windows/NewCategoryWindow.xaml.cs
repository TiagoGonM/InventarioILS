using InventarioILS.Model.Storage;
using InventarioILS.Model;
using InventarioILS.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.Windows
{
    public partial class NewCategoryWindow : Window
    {
        readonly ItemSubCategories subcategories = ItemSubCategories.Instance;
        readonly ItemCategories categories = ItemCategories.Instance;

        public NewCategoryWindow()
        {
            InitializeComponent();

            DataContext = new
            {
                SubcategoryList = subcategories.Items
            };
        }

        private async void CreateCategoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedIds = LinkedSubcategoriesComboBox.SelectedItems.Cast<ItemMisc>().Select(subcat => subcat.Id).ToHashSet();
            var newCategory = new ItemMisc(CategoryNameInput.Text.ToLower(), CategoryShorthandInput.Text);

            await CategoryService.RegisterCategoryAsync(newCategory, selectedIds).ConfigureAwait(false);

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

            CreateCategoryBtn.IsEnabled = !exists;
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
            CreateCategoryBtn.IsEnabled = !exists;
        }
    }
}
