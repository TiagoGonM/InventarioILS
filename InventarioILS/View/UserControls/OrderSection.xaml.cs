using InventarioILS.Model;
using System.Linq;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para OrderSection.xaml
    /// </summary>
    public partial class OrderSection : UserControl
    {
        Orders _orders = null;

        public OrderSection()
        {
            InitializeComponent();
            _orders = new Orders();
           
            _orders.Load();
            
            this.DataContext = new
            {
                Count = _orders.Items.Count(),
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
            new ViewOrderPopup().Show();
        }
    }
}
