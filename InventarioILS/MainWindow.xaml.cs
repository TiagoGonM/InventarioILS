using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InventarioILS.Model;
using System.Windows.Media;
using Windows.UI.Text;

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
        //readonly DbConnection client;
        readonly StockItems client;

        bool sidebarCollapsed = false;

        GridLength prevWidth;

        bool bottomBarCollapsed = true;

        int orderSectionHeight = 400;

        bool isStock = true;

        public MainWindow()
        {
            InitializeComponent();
            //client = new DbConnection();
            client = new StockItems();
        }

        private void AddItemBtn_Click(object sender, RoutedEventArgs e)
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

        public void SetItems()
        {
            //items.Clear();

            ////items = !isStock
            ////    ? client.GetItems(appliedFilters).ToObservableCollection<Item>()
            ////    : client.GetStockItems(appliedFilters).ToObservableCollection<Item>();


            //ItemView.ItemsSource = items;

            client.Load();
            ItemView.ItemsSource = client.Items;
        }

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ClassComboBox.SelectedItem;

            if (selectedItem == null) return;

            var content = ((ComboBoxItem)selectedItem).Content;

            client.AddFilter(Filters.CLASS_NAME, content.ToString());

            SetItems();
        }

        private void ProductCodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProductCodeInput.Text))
            {
                client.RemoveFilter(Filters.PRODUCT_CODE);
                SetItems();
                return;
            }

            client.AddFilter(Filters.PRODUCT_CODE, ProductCodeInput.Text);

            SetItems();
        }

        private void KeywordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(KeywordInput.Text))
            {
                client.RemoveFilter(Filters.KEYWORD);
                SetItems();
                return;
            }

            client.AddFilter(Filters.KEYWORD, KeywordInput.Text);

            SetItems();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            client.ClearFilters();

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

        private void InventoryTabBtn_Click(object sender, RoutedEventArgs e)
        {
            InventoryTabBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF9BE8D6");
            InventoryTabBtn.FontWeight = FontWeights.Bold;

            OrderTabBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE0E0E0");
            OrderTabBtn.FontWeight = FontWeights.Normal;

            SettingsBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE0E0E0");
            SettingsBtn.FontWeight = FontWeights.Normal;

            AddOrderBtn.Visibility = Visibility.Collapsed;
            AddItemBtn.Visibility = Visibility.Visible;
            RightGrid.RowDefinitions[1].Height = new GridLength(0);

            Grid.SetRow(CollapsedButtonBar, 0);

            bottomBarCollapsed = true;
        }

        private void OrderTabBtn_Click(object sender, RoutedEventArgs e)
        {
            InventoryTabBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE0E0E0");
            InventoryTabBtn.FontWeight = FontWeights.Normal;

            SettingsBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE0E0E0");
            SettingsBtn.FontWeight = FontWeights.Normal;

            OrderTabBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF9BE8D6");
            OrderTabBtn.FontWeight = FontWeights.Bold;

            var row = Grid.GetRow(OrderListSection);
            RightGrid.RowDefinitions[row].Height = new GridLength(orderSectionHeight);
            AddOrderBtn.Visibility = Visibility.Visible;
            AddItemBtn.Visibility = Visibility.Collapsed;

            Grid.SetRow(CollapsedButtonBar, 1);

            bottomBarCollapsed = false;
        }

        // TODO: Refactor this method to reduce complexity
        private void CollapseSidebarBtn_Click(object sender, RoutedEventArgs e)
        {
            var col = Grid.GetColumn(CollapseSidebarBtn);
            var colDef = MainGrid.ColumnDefinitions[col];

            colDef.MinWidth = 0;
            
            if (!sidebarCollapsed) prevWidth = colDef.Width;
            
            colDef.Width = new GridLength(!sidebarCollapsed ? 0 : 320);

            sidebarCollapsed = !sidebarCollapsed;

            if (sidebarCollapsed)
            {
                if (!bottomBarCollapsed)
                {
                    AddOrderCollapsedBtn.Visibility = Visibility.Visible;
                    AddItemCollapsedBtn.Visibility = Visibility.Collapsed;
                } else
                {
                    AddOrderCollapsedBtn.Visibility = Visibility.Collapsed;
                    AddItemCollapsedBtn.Visibility = Visibility.Visible;
                }
            } else
            {
                AddOrderCollapsedBtn.Visibility = Visibility.Collapsed;
                AddItemCollapsedBtn.Visibility = Visibility.Collapsed;
            }

            DecollapseSidebarBtn.Visibility = !sidebarCollapsed ? Visibility.Hidden : Visibility.Visible;
            if (!sidebarCollapsed)
            {
                colDef.MinWidth = 220;
                colDef.Width = prevWidth;
            }
        }

        private void AddOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelOrderFormBtn.Visibility = Visibility.Visible;
            AddOrderBtn.Visibility = Visibility.Collapsed;

            ShowAddOrderSection(true);
        }

        private void CancelOrderFormBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelOrderFormBtn.Visibility = Visibility.Collapsed;
            AddOrderBtn.Visibility = Visibility.Visible;

            ShowAddOrderSection(false);
        }

        private void AddOrderCollapsedBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelNewOrderFormCollapsedBtn.Visibility = Visibility.Visible;
            AddOrderCollapsedBtn.Visibility = Visibility.Collapsed;

            ShowAddOrderSection(true);
        }

        private void CancelNewOrderFormCollapsedBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelNewOrderFormCollapsedBtn.Visibility = Visibility.Collapsed;
            AddOrderCollapsedBtn.Visibility = Visibility.Visible;

            ShowAddOrderSection(false);
        }

        private void ShowAddOrderSection(bool show)
        {
            if (!show)
            {
                OrderListSection.Visibility = Visibility.Visible;
                AddOrderSection.Visibility = Visibility.Collapsed;
                return;
            }

            OrderListSection.Visibility = Visibility.Collapsed;
            AddOrderSection.Visibility = Visibility.Visible;
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            InventoryTabBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE0E0E0");
            InventoryTabBtn.FontWeight = FontWeights.Normal;

            OrderTabBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE0E0E0");
            OrderTabBtn.FontWeight = FontWeights.Normal;

            SettingsBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF9BE8D6");
            SettingsBtn.FontWeight = FontWeights.Bold;

            InventoryTabBtn.IsEnabled = false;
            OrderTabBtn.IsEnabled = false;

            ItemView.Visibility = Visibility.Collapsed;
            SettingsSection.Visibility = Visibility.Visible;
        }

        private void SettingsBackBtn_Click(object sender, RoutedEventArgs e)
        {
            InventoryTabBtn.IsEnabled = true;
            OrderTabBtn.IsEnabled = true;

            ItemView.Visibility = Visibility.Visible;
            SettingsSection.Visibility = Visibility.Collapsed;
        }
    }
}
