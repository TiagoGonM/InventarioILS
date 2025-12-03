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
        Orders _orders = null;

        public OrderPanel()
        {
            InitializeComponent();
            _orders = new Orders();
           
            _orders.Load();
            
            this.DataContext = new
            {
                Count = _orders.Items.Count,
                OrderList = _orders.Items
            };
        }

        internal Orders OrderList
        {
            set { _orders = value; }
            get { return _orders; }
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
