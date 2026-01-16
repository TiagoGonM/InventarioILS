using InventarioILS.Model.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InventarioILS.View.Windows
{
    /// <summary>
    /// Lógica de interacción para NewShipmentStateWindow.xaml
    /// </summary>
    public partial class NewShipmentStateWindow : Window
    {
        readonly ShipmentStates shipStateStorage = ShipmentStates.Instance;

        public NewShipmentStateWindow()
        {
            InitializeComponent();
        }

        public void Input_TextChanged(object sender, RoutedEventArgs e)
        {
            string input = Input.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                SubmitBtn.IsEnabled = false;
                ErrorText.Visibility = Visibility.Collapsed;
                return;
            }

            bool exists = shipStateStorage.Items.Any(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

            ErrorText.Visibility = !exists ? Visibility.Collapsed : Visibility.Visible;
            if (exists) Input.Focus();

            SubmitBtn.IsEnabled = !exists;
        }

        public async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            await shipStateStorage.AddAsync(Input.Text.ToLower());
            Close();
        }
    }
}
