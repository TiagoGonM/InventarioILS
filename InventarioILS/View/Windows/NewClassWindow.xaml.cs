using InventarioILS.Model.Storage;
using InventarioILS.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.Windows
{
    public partial class NewClassWindow : Window
    {
        readonly ItemClasses classes = ItemClasses.Instance;

        public NewClassWindow()
        {
            InitializeComponent();
        }

        private async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            await classes.AddAsync(new ItemMisc(ClassNameInput.Text.ToLower()));

            Close();
        }

        private void ClassNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = ClassNameInput.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                SubmitBtn.IsEnabled = false;
                ErrorText.Visibility = Visibility.Collapsed;
                return;
            }

            bool exists = classes.Items.Any(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

            ErrorText.Visibility = !exists ? Visibility.Collapsed : Visibility.Visible;
            SubmitBtn.IsEnabled = !exists;
        }
    }
}
