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

            var orderItems = new OrderItems();
            orderItems.LoadSingle(1);
            DataContext = new
            {
                OrderName = "Pedido #1",
                OrderDescription = "pedido de prueba",
                OrderCreationDate = DateTime.Now.ToString("dd/MM/yyyy"),
                orderItems.Items
            };
        }
    }
}
