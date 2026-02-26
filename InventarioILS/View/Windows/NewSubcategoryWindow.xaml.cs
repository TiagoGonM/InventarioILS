using InventarioILS.Model.Storage;
using InventarioILS.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.Windows
{
    public partial class NewSubcategoryWindow : Window
    {
        readonly static ItemSubcategories subcategoryStorage = ItemSubcategories.Instance;

        public NewSubcategoryWindow()
        {
            subcategoryStorage.Load();

            InitializeComponent();
        }

        private async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            var newSubcategory = new ItemMisc(SubcategoryNameInput.Text.ToLower(), SubcategoryShorthandInput.Text);

            try
            {
                await subcategoryStorage.AddAsync(newSubcategory).ConfigureAwait(false);
            } catch (Exception ex)
            {
                await StatusManager.Instance.UpdateMessageStatusAsync($"Error al crear la subcategoria: {ex.Message}", StatusManager.MessageType.ERROR);
                return;
            }

            Close();
            await StatusManager.Instance.UpdateMessageStatusAsync($"Subcategoria '{newSubcategory.Name}' creada exitosamente.", StatusManager.MessageType.SUCCESS);
        }

        private void SubcategoryNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = SubcategoryNameInput.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                SubmitBtn.IsEnabled = false;
                SubcategoryNameErrorText.Visibility = Visibility.Collapsed;
                return;
            }

            bool exists = subcategoryStorage.Items.Any(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

            SubcategoryNameErrorText.Visibility = !exists ? Visibility.Collapsed : Visibility.Visible;
            if (exists) SubcategoryNameInput.Focus();

            SubmitBtn.IsEnabled = !exists;
        }

        private void SubcategoryShorthandInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = SubcategoryShorthandInput.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                SubcategoryShorthandErrorText.Visibility = Visibility.Collapsed;
                return;
            }

            bool exists = subcategoryStorage.Items.Any(subcat => !string.IsNullOrEmpty(subcat.Shorthand) && subcat.Shorthand.Equals(input, StringComparison.OrdinalIgnoreCase));

            SubcategoryShorthandErrorText.Visibility = !exists ? Visibility.Collapsed : Visibility.Visible;
            if (exists) SubcategoryShorthandInput.Focus();

            SubmitBtn.IsEnabled = !exists;
        }
    }
}
