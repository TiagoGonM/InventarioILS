using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using InventarioILS.Model;

namespace InventarioILS
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DbConnection client;

        Dictionary<string, string> appliedFilters = new Dictionary<string, string>();

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
            var filter = client.GetStockItemsByClass(((ComboBoxItem)ClassComboBox.SelectedItem).Content.ToString());
            ItemView.SetItemsSource(client.GetStockItems2(appliedFilters));
            appliedFilters.Add("item-class", ((ComboBoxItem)ClassComboBox.SelectedItem).Content.ToString());
        }

        private void ProductCodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProductCodeInput.Text))
            {
                appliedFilters.Remove("product-code");
                ItemView.SetItemsSource(client.GetStockItems());
                return;
            }

            ItemView.SetItemsSource(client.GetStockItems2(appliedFilters));
            appliedFilters.Add("product-code", ProductCodeInput.Text);
        }

        private void KeywordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProductCodeInput.Text))
            {
                appliedFilters.Remove("keyword");
                ItemView.SetItemsSource(client.GetStockItems());
                return;
            }

            ItemView.SetItemsSource(client.GetStockItems2(appliedFilters));
            appliedFilters.Add("keyword", KeywordInput.Text);
        }

        private void clearFilters_Click(object sender, RoutedEventArgs e)
        {
            ItemView.SetItemsSource(client.GetStockItems2(appliedFilters));
        }

        private void ItemView_Loaded(object sender, RoutedEventArgs e)
        {
            ItemView.SetItemsSource(client.GetStockItems2(appliedFilters));
        }
    }
}
