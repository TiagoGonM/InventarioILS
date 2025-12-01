using System.Linq;
using System.Windows.Controls;

using InventarioILS.Model;
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
            
            this.DataContext = new
            {
                Count = orders.Items.Count,
                OrderList = orders.Items
            };
        }

        private void OrderCard_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is OrderCard card)
            {
                var order = new Order((int)card.Tag, card.Title, card.Description, card.CreationDate);

                new ViewOrderWindow(order).Show();
            }
        }
    }
}
