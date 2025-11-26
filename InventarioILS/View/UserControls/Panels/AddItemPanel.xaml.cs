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
        static readonly StockItems Items = StockItems.Instance;

        public AddItemSection()
        {
            InitializeComponent();
        }

        private void AddItem_OnConfirm(object sender, StockItemEventArgs e)
        {
            for (int i = 0; i < e.Item.Quantity; i++)
                Items.Add(e.Item);
        }
    }
}
