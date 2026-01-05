using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.View.UserControls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.Windows
{
    public partial class ViewOrderWindow : Window, IDisposable
    {
        readonly ShipmentStates shipmentStates = ShipmentStates.Instance;
        readonly OrderItems orderItems = OrderItems.Instance;
        
        readonly ObservableCollection<OrderItem> receivedItems = [];

        public ViewOrderWindow()
        {
            InitializeComponent();
        }

        public ObservableCollection<ItemMisc> ShipmentStateList => shipmentStates.Items;

        public static async Task<ViewOrderWindow> CreateAsync(Order order)
        {
            ViewOrderWindow window = new();
            await window.orderItems.LoadSingleAsync(order.Id);

            
            await window.shipmentStates.LoadAsync();

            window.DataContext = new
            {
                OrderName = order.Name,
                OrderDescription = order.Description,
                OrderCreationDate = order.CreatedAt.ToString("dd/MM/yyyy"),
                window.ShipmentStateList,
                window.orderItems.Items
            };

            return window;
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
            Dispose();
            new ConfirmReceivedItemsWindow(receivedItems).Show();
        }

        public void Dispose()
        {
            orderItems.Items.Clear();
            GC.SuppressFinalize(this);
        }

        private async Task ShipmentStateComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;
            var itemCtx = combo.DataContext as OrderItem;

            await orderItems.UpdateAsync(itemCtx); // TODO
        }
    }
}
