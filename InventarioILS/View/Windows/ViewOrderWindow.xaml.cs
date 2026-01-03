using InventarioILS.Model;
using InventarioILS.Model.Storage;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.Windows
{
    public partial class ViewOrderWindow : Window
    {
        readonly ShipmentStates shipmentStates = ShipmentStates.Instance;
        readonly OrderItems orderItems = OrderItems.Instance;
        
        readonly ObservableCollection<OrderItem> receivedItems = [];

        public ViewOrderWindow()
        {
            InitializeComponent();
        }

        public ObservableCollection<ItemMisc> ShipmentStateList => shipmentStates.Items;

        public ViewOrderWindow(Order order) : this()
        {
            orderItems.LoadSingle(order.Id);
            shipmentStates.Load();

            DataContext = new
            {
                OrderName = order.Name,
                OrderDescription = order.Description,
                OrderCreationDate = order.CreatedAt.ToString("dd/MM/yyyy"),
                ShipmentStateList,
                orderItems.Items
            };
        }

        private void Received_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var ctxItem = checkbox.DataContext as OrderItem;

            receivedItems.Add(ctxItem);
            ConfirmBtn.IsEnabled = true;
        }

        private void Received_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var ctxItem = checkbox.DataContext as OrderItem;

            var storedItem = receivedItems.First(item => item.Id == ctxItem.Id);
            receivedItems.RemoveAt(receivedItems.IndexOf(storedItem));

            ConfirmBtn.IsEnabled = receivedItems.Count == 0;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
            new ConfirmReceivedItemsWindow(receivedItems).Show();
        }
    }
}
