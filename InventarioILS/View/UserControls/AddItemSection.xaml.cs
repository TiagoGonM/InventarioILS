using InventarioILS.Model;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para AddItemSection.xaml
    /// </summary>
    public partial class AddItemSection : UserControl
    {
        StockItems Items => StockItems.Instance;

        public AddItemSection()
        {
            InitializeComponent();
        }

        private void AddItem_OnConfirm(object sender, StockItemEventArgs e)
        {
            MessageBox.Show($"Event fired!, got {e.Item.Description}");
            Items.Add(e.Item);
        }
    }
}
