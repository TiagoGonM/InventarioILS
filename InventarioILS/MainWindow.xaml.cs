using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InventarioILS.Model;
using System.Windows.Media;
using System;
using InventarioILS.View.UserControls;

namespace InventarioILS
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly StockItems items;
        readonly ItemClasses itemClasses;

        bool sidebarCollapsed = false;

        GridLength prevWidth;

        bool bottomBarCollapsed = true;

        int _bottomBarHeight = 0;

        int defaultBottomBarHeight = 400;

        int BottomBarHeight { 
            get => _bottomBarHeight; 
            set => _bottomBarHeight = value; 
        }

        bool isStock = true;

        public object SelectedClassItem { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            items = StockItems.Instance;
            itemClasses = ItemClasses.Instance;

            DataContext = new
            {
                BottomBarHeight,
                items.Items,
                ItemClassList = itemClasses.Items,
            };
        }

        private void AddItemBtn_Click(object sender, RoutedEventArgs e)
        {
            AddItemSection.Visibility = Visibility.Visible;
            OrderListSection.Visibility = Visibility.Collapsed;
            ShowBottomBar();
        }

        public void SetItems()
        {
            items.Load();
        }

        private void ClassComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var selectedItem = (ItemMisc)combo.SelectedItem;
            
            if (selectedItem == null) return;

            var content = selectedItem.Name;

            //MessageBox.Show("Contenido:" + content);

            items.QueryFilters.AddFilter(StockItems.Filters.CLASS_NAME, content.ToString());

            SetItems();
        }

        private void ProductCodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProductCodeInput.Text))
            {
                items.QueryFilters.RemoveFilter(StockItems.Filters.PRODUCT_CODE);
                SetItems();
                return;
            }

            items.QueryFilters.AddFilter(StockItems.Filters.PRODUCT_CODE, ProductCodeInput.Text);

            SetItems();
        }

        private void KeywordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(KeywordInput.Text))
            {
                items.QueryFilters.RemoveFilter(StockItems.Filters.KEYWORD);
                SetItems();
                return;
            }

            items.QueryFilters.AddFilter(StockItems.Filters.KEYWORD, KeywordInput.Text);

            SetItems();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            items.QueryFilters.ClearFilters();

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

            ShowBottomBar(false);

            Grid.SetRow(CollapsedButtonBar, 0);
        }

        private void ShowBottomBar(bool show = true)
        {
            var row = Grid.GetRow(OrderListSection);
            RightGrid.RowDefinitions[row].Height = new GridLength(show ? defaultBottomBarHeight : 0);

            Grid.SetRow(CollapsedButtonBar, show ? 1 : 0);

            bottomBarCollapsed = show;
        }

        private void OrderTabBtn_Click(object sender, RoutedEventArgs e)
        {
            InventoryTabBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE0E0E0");
            InventoryTabBtn.FontWeight = FontWeights.Normal;

            SettingsBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE0E0E0");
            SettingsBtn.FontWeight = FontWeights.Normal;

            OrderTabBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF9BE8D6");
            OrderTabBtn.FontWeight = FontWeights.Bold;

            ShowBottomBar();

            AddOrderBtn.Visibility = Visibility.Visible;
            AddItemBtn.Visibility = Visibility.Collapsed;
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
