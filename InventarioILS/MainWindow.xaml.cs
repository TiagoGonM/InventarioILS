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

        // TODO: Avoid code duplication
        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassComboBox.SelectedItem == null) return;

            if (!appliedFilters.ContainsKey("item-class"))
                appliedFilters.Add("item-class", ((ComboBoxItem)ClassComboBox.SelectedItem).Content.ToString());
            else
                appliedFilters["item-class"] = ((ComboBoxItem)ClassComboBox.SelectedItem).Content.ToString();
            
            ItemView.SetItemsSource(client.GetStockItems(appliedFilters));
        }

        private void ProductCodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProductCodeInput.Text))
            {
                appliedFilters.Remove("product-code");
                ItemView.SetItemsSource(client.GetStockItems(appliedFilters));
                return;
            }

            if (!appliedFilters.ContainsKey("product-code"))
                appliedFilters.Add("product-code", ProductCodeInput.Text);
            else
                appliedFilters["product-code"] = ProductCodeInput.Text;
                
            ItemView.SetItemsSource(client.GetStockItems(appliedFilters));
            //ItemView.SetItemsSource(client.GetStockItemsByCode(ProductCodeInput.Text));
        }

        private void KeywordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(KeywordInput.Text))
            {
                appliedFilters.Remove("keyword");
                ItemView.SetItemsSource(client.GetStockItems(appliedFilters));
                return;
            }

            if (!appliedFilters.ContainsKey("keyword"))
                appliedFilters.Add("keyword", KeywordInput.Text);
            else
                appliedFilters["keyword"] = KeywordInput.Text;
            
            ItemView.SetItemsSource(client.GetStockItems(appliedFilters));
            //ItemView.SetItemsSource(client.GetStockItemsByKeyword(KeywordInput.Text));
        }

        private void clearFilters_Click(object sender, RoutedEventArgs e)
        {
            appliedFilters.Clear();
            ClassComboBox.SelectedItem = null;
            ProductCodeInput.Text = string.Empty;
            KeywordInput.Text = string.Empty;
            ItemView.SetItemsSource(client.GetStockItems(appliedFilters));
            //ItemView.SetItemsSource(client.GetStockItems());
        }

        private void ItemView_Loaded(object sender, RoutedEventArgs e)
        {
            ItemView.SetItemsSource(client.GetStockItems(appliedFilters));
            //ItemView.SetItemsSource(client.GetStockItems());
        }

        private void ShowNoStockItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
