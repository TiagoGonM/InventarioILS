using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.View.UserControls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.Windows
{
    public partial class NewStateWindow : Window
    {
        readonly ItemStates states = ItemStates.Instance;
        readonly ItemClasses classes = ItemClasses.Instance;

        uint selectedClassId;

        public NewStateWindow()
        {
            InitializeComponent();

            DataContext = new
            {
                ClassList = classes.Items
            };
        }

        private async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await states.AddAsync(new ItemMisc(StateNameInput.Text.ToLower()), selectedClassId);
            } catch (Exception ex)
            {
                await StatusManager.Instance.UpdateMessageStatusAsync($"Error al intentar crear el estado: {ex.Message}", StatusManager.MessageType.ERROR);
                return;
            }

            Close();
            await StatusManager.Instance.UpdateMessageStatusAsync($"Estado '{StateNameInput.Text}' creado exitosamente.", StatusManager.MessageType.SUCCESS);
        }

        private void StateNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = StateNameInput.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                SubmitBtn.IsEnabled = false;
                ErrorText.Visibility = Visibility.Collapsed;
                return;
            } 

            bool exists = states.Items.Any(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
            
            ErrorText.Visibility = !exists ? Visibility.Collapsed : Visibility.Visible;
            if (exists) StateNameInput.Focus();

            SubmitBtn.IsEnabled = !exists;
        }

        private void ClassComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var itemClass = (ItemMisc)combo.SelectedItem;

            if (itemClass == null) return;

            selectedClassId = itemClass.Id;
        }
    }
}
