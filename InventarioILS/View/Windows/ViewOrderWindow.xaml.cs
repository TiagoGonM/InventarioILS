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
        static readonly ShipmentStates shipmentStates = ShipmentStates.Instance;
        static readonly OrderItems orderItems = OrderItems.Instance;
        Order currentOrder;
        
        readonly ObservableCollection<OrderItem> receivedItems = [];

        bool submitted = false;

        public ViewOrderWindow()
        {
            InitializeComponent();
            Closed += ViewOrderWindow_Closed;
        }

        public static async Task<ViewOrderWindow> CreateAsync(Order order)
        {
            ViewOrderWindow window = new();
            await orderItems.LoadSingleAsync(order.Id);
            await shipmentStates.LoadAsync();

            window.currentOrder = order;

            if (orderItems.Items.All(item => item.Received == true)) 
                await Orders.Instance.MarkAsDoneAsync(order.Id);

            window.DataContext = new
            {
                OrderName = order.Name,
                OrderDescription = order.Description,
                OrderCreationDate = order.CreatedAt.ToString("dd/MM/yyyy"),
                ShipmentStateList = shipmentStates.Items,
                orderItems.Items
            };

            return window;
        }

        private void Received_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var ctxItem = checkbox.DataContext as OrderItem;

            if (ctxItem.Received) return;

            receivedItems.Add(ctxItem);
            ConfirmBtn.IsEnabled = true;
        }

        private void Received_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var ctxItem = checkbox.DataContext as OrderItem;

            if (ctxItem.Received) return;

            var storedItem = receivedItems.First(item => item.Id == ctxItem.Id);
            receivedItems.RemoveAt(receivedItems.IndexOf(storedItem));

            ConfirmBtn.IsEnabled = receivedItems.Count > 0;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            submitted = true;
            Close();
            new ConfirmReceivedItemsWindow(receivedItems).Show();
        }

        private void ViewOrderWindow_Closed(object sender, EventArgs e)
        {
            Dispose();
        }

        private async void ShipmentStateComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;
            
            var item = (ItemMisc)combo.SelectedItem;

            if (item is null) return;

            var newId = item.Id;

            var itemCtx = combo.DataContext as OrderItem;

            await orderItems.UpdateAsync(itemCtx.ProductCode, newId, currentOrder.Id);
        }

        public void Dispose()
        {
            // Limpiar event handlers
            Closed -= ViewOrderWindow_Closed;

            // Limpiar referencias del DataContext
            DataContext = null;

            // Limpiar colecciones
            if (!submitted)
            {
                orderItems.Items.Clear();
                receivedItems.Clear();
            }

            // Limpiar referencias
            currentOrder = null;

            GC.SuppressFinalize(this);
        }
    }
}
