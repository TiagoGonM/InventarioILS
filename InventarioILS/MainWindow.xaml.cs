using InventarioILS.Model;
using InventarioILS.View.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventarioILS
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DbConnection client;

        public MainWindow()
        {
            InitializeComponent();
            client = new DbConnection();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //List<StockItem> items = new List<StockItem>();
            //items.Add(new StockItem("R-100-K", "Resistencia", "Estandar", "Resistencia de 100K ohm", Class.INSUMO, State.INSUMO_SUFICIENTE, "Cajón 1", 5));
            //ItemView.SetItemsSource(items);
            return;
        }

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show(((ComboBoxItem)ClassComboBox.SelectedItem).Content.ToString());
            var filter = client.GetStockItemsByClass(((ComboBoxItem)ClassComboBox.SelectedItem).Content.ToString());
            ItemView.SetItemsSource(filter);
        }

        private void ProductCodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ItemView.SetItemsSource(client.GetStockItemsByCode(ProductCodeInput.Text));
        }

        private void KeywordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ItemView.SetItemsSource(client.GetStockItemsByKeyword(KeywordInput.Text));
        }
    }
}
