using InventarioILS.Model;
using InventarioILS.Model.Storage;
using System.Windows;

namespace InventarioILS.View.Windows
{
    /// <summary>
    /// Lógica de interacción para ViewOrderPopup.xaml
    /// </summary>
    public partial class ViewOrderWindow : Window
    {
        public ViewOrderWindow()
        {
            InitializeComponent();
        }

        public ViewOrderWindow(Order order) : this()
        {
            var orderItems = new OrderItems();
            orderItems.LoadSingle(order.Id);

            DataContext = new
            {
                OrderName = order.Name,
                OrderDescription = order.Description,
                OrderCreationDate = order.CreatedAt.ToString("dd/MM/yyyy"),
                orderItems.Items
            };
        }
    }
}
