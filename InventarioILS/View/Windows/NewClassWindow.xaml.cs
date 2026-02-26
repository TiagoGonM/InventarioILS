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
            classes.Load();

            InitializeComponent();
        }

        private async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await classes.AddAsync(new ItemMisc(ClassNameInput.Text.ToLower()));
            } catch (Exception ex)
            {
                await StatusManager.Instance.UpdateMessageStatusAsync($"Error intentando crear la clase: {ex.Message}", StatusManager.MessageType.ERROR);
                return;
            }

            Close();
            await StatusManager.Instance.UpdateMessageStatusAsync($"Clase '{ClassNameInput.Text}' creada exitosamente.", StatusManager.MessageType.SUCCESS);
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
            if (exists) ClassNameInput.Focus();

            SubmitBtn.IsEnabled = !exists;
        }
    }
}
