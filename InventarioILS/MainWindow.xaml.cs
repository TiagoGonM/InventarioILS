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

        readonly Map<string, string> appliedFilters;

        bool isStock = true;

        public MainWindow()
        {
            InitializeComponent();
            client = new DbConnection();
            appliedFilters = new Map<string, string>();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //List<StockItem> items = new List<StockItem>();
            //items.Add(new StockItem("R-100-K", "Resistencia", "Estandar", "Resistencia de 100K ohm", Class.INSUMO, State.INSUMO_SUFICIENTE, "Cajón 1", 5));
            //ItemView.SetItemsSource(items);
            return;
        }

        public class Map<TKey, TValue> : Dictionary<TKey, TValue>
        {
            public void AddOrUpdate(TKey key, TValue value)
            {
                if (!this.ContainsKey(key))
                    this.Add(key, value);
                else 
                    this[key] = value;
            }
        }

        public void SetItems()
        {
            if (!isStock)
                ItemView.ItemsSource = client.GetItems(appliedFilters);
            else
                ItemView.ItemsSource = client.GetStockItems(appliedFilters);
        }

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassComboBox.SelectedItem == null) return;

            appliedFilters.AddOrUpdate("className", ((ComboBoxItem)ClassComboBox.SelectedItem).Content.ToString());
            
            SetItems();
        }

        private void ProductCodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProductCodeInput.Text))
            {
                appliedFilters.Remove("productCode");
                SetItems();
                return;
            }

            appliedFilters.AddOrUpdate("productCode", ProductCodeInput.Text);

            SetItems();
        }

        private void KeywordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(KeywordInput.Text))
            {
                appliedFilters.Remove("keyword");
                SetItems();
                return;
            }

            appliedFilters.AddOrUpdate("keyword", KeywordInput.Text);

            SetItems();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            appliedFilters.Clear();

            ClassComboBox.SelectedItem = null;
            ProductCodeInput.Text = string.Empty;
            KeywordInput.Text = string.Empty;

            SetItems();
        }

        private void ItemView_Loaded(object sender, RoutedEventArgs e)
        {
            SetItems();
        }

        private void ShowNonStockItems_Click(object sender, RoutedEventArgs e)
        {
            isStock = !ShowNonStockItems.IsChecked ?? true;

            SetItems();
        }
    }
}
