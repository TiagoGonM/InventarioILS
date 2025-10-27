using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InventarioILS.Model;
using Microsoft.Data.Sqlite;
using System.Windows.Media;

namespace InventarioILS
{
    public static class ObservableCollectionExt
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }
    }

    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DbConnection client;

        bool sidebarCollapsed = false;

        readonly Map<string, string> appliedFilters;
        ObservableCollection<Item> items;

        bool isStock = true;

        public MainWindow()
        {
            InitializeComponent();
            client = new DbConnection();
            appliedFilters = new Map<string, string>();
            items = new ObservableCollection<Item>();
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddItemPopup.IsOpen = true;
            var item = new StockItem("R-10", "Resistencia", "estandar / no definido", "Resistencia de 10 ohm", Class.INSUMO, "suficientes", "Cajón 1", 5);

            //items.Add(item);

            //try
            //{
            //    client.SaveItem(item);
            //} catch (SqliteException ex)
            //{
            //    UpdateMessageStatus($"Error al guardar el ítem: {ex.Message}", Brushes.Red);
            //    return;
            //}

            //UpdateMessageStatus("Ítem guardado exitosamente.", Brushes.Green);
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
            items.Clear();

            items = !isStock
                ? client.GetItems(appliedFilters).ToObservableCollection<Item>()
                : client.GetStockItems(appliedFilters).ToObservableCollection<Item>();

            ItemView.ItemsSource = items;

        }

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ClassComboBox.SelectedItem;

            if (selectedItem == null) return;

            var content = ((ComboBoxItem)selectedItem).Content;

            appliedFilters.AddOrUpdate("className", content.ToString());

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

        private async void UpdateMessageStatus(string message, Brush color)
        {
            StatusMessageLabel.Foreground = color;
            StatusMessageLabel.Content = message;
            StatusMessageLabel.Visibility = Visibility.Visible;

            await Task.Delay(5000);
            
            StatusMessageLabel.Visibility = Visibility.Hidden;
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            AddItemPopup.IsOpen = false;
        }

        private void SaveEdit_Click(object sender, RoutedEventArgs e)
        {
            AddItemPopup.IsOpen = false;
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            Grid.GetRow(OrderLabel);
            RightGrid.RowDefinitions[1].Height = new GridLength(200);
        }

        private void CollapseSidebarBtn_Click(object sender, RoutedEventArgs e)
        {
            var col = Grid.GetColumn(CollapseSidebarBtn);
            var colDef = MainGrid.ColumnDefinitions[col];

            colDef.Width = new GridLength(!sidebarCollapsed ? 0 : 320);
            sidebarCollapsed = !sidebarCollapsed;
            AddItemCollapsedBtn.Visibility = !sidebarCollapsed ? Visibility.Hidden : Visibility.Visible;
            DecollapseSidebarBtn.Visibility = !sidebarCollapsed ? Visibility.Hidden : Visibility.Visible;
        }

        private void Sidebar_ProductCodeTextChanged(object sender, TextChangedEventArgs e)
        {
            MessageBox.Show("Text changed!");
        }

        private void Button_Click_1()
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
