using System.Windows.Controls;

using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.View.Windows;

namespace InventarioILS.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para OrderPanel.xaml
    /// </summary>
    public partial class OrderPanel : UserControl
    {
        Orders orders = Orders.Instance;

        public OrderPanel()
        {
            InitializeComponent();
            
            DataContext = new
            {
                orders.Items.Count,
                OrderList = orders.Items
            };
        }

        private async void OrderCard_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is OrderCard card)
            {
                var order = new Order((uint)card.Tag, card.Title, card.Description, card.CreationDate);

                var window = await ViewOrderWindow.CreateAsync(order).ConfigureAwait(false);
                window.Show();
            }
        }
    }
}
