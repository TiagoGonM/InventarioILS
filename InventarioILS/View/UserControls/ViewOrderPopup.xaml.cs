using InventarioILS.Model;
using System;
using System.Windows;

namespace InventarioILS.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para ViewOrderPopup.xaml
    /// </summary>
    public partial class ViewOrderPopup : Window
    {
        public ViewOrderPopup()
        {
            InitializeComponent();
        }

        public ViewOrderPopup(Order order) : this()
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
