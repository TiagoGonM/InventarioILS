using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InventarioILS.Model;
using System.Windows.Media;
using System;
using InventarioILS.View.UserControls;
using InventarioILS.Model.Storage;

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

        double _bottomBarHeight = 0;

        double defaultBottomBarHeight = 400;

        enum AppPages
        {
            INVENTORY,
            ORDER,
            SETTINGS
        }

        AppPages currentWindow = AppPages.INVENTORY;

        bool isStock = true;

        readonly BottomBarManager bottomBarManager = BottomBarManager.Instance;

        public object SelectedClassItem { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            items = StockItems.Instance;
            itemClasses = ItemClasses.Instance;
            
            StatusManager.Instance.Initialize(StatusMessageLabel); // Initialize app status messages on the bottom
            bottomBarManager.Initialize(BottomBarContent, BottomRow);

            DataContext = new
            {
                items.Items,
                ItemClassList = itemClasses.Items,
            };
        }

        AddItemPanel addItemPanel;
        private void AddItemBtn_Click(object sender, RoutedEventArgs e)
        {
            addItemPanel = new AddItemPanel();
            addItemPanel.OnSuccess += CancelAction;
            StartActionWith(addItemPanel);

            CancelBtn.Click += AddItemBtn_CancelAction;
        }
        
        private void AddItemBtn_CancelAction(object sender, RoutedEventArgs e)
        {
            CancelAction();
            CancelBtn.Click -= AddItemBtn_CancelAction;
        }

        AddOrderPanel addOrderPanel;
        private void AddOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            addOrderPanel = new AddOrderPanel();
            addOrderPanel.OnSuccess += CancelAction;
            
            StartActionWith(addOrderPanel);

            CancelBtn.Click += AddOrderBtn_CancelAction;
        }

        private void AddOrderBtn_CancelAction(object sender, RoutedEventArgs e)
        {
            CancelAction();

            CancelBtn.Click -= AddOrderBtn_CancelAction;
            OrderTabBtn_Click(sender, e);
        }

        private void StartActionWith(UserControl control)
        {
            bottomBarManager.CurrentControlContent = control;
            ShowBottomBar();
            ShowCancelBtn();
        }

        private void CancelAction()
        {
            bottomBarManager.CleanControlContent();
            ShowBottomBar(false);
            ShowCancelBtn(false);
        }

        public async Task SetItems()
        {
            if (isStock) await items.LoadAsync().ConfigureAwait(false);
            else await items.LoadNoStockAsync().ConfigureAwait(false);
        }

        private async void ClassComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;
            var selectedItem = (ItemMisc)combo.SelectedItem;
            if (selectedItem == null) return;

            var content = selectedItem.Name;

            items.QueryFilters.AddFilter(StockItems.Filters.CLASS_NAME, content.ToString());

            await SetItems();
        }

        private async void ProductCodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProductCodeInput.Text))
            {
                items.QueryFilters.RemoveFilter(StockItems.Filters.PRODUCT_CODE);
                await SetItems();
                return;
            }

            items.QueryFilters.AddFilter(StockItems.Filters.PRODUCT_CODE, ProductCodeInput.Text);

            await SetItems();
        }

        private async void KeywordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(KeywordInput.Text))
            {
                items.QueryFilters.RemoveFilter(StockItems.Filters.KEYWORD);
                await SetItems();
                return;
            }

            items.QueryFilters.AddFilter(StockItems.Filters.KEYWORD, KeywordInput.Text);

            await SetItems();
        }

        private async void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            items.QueryFilters.ClearFilters();

            ClassComboBox.SelectedItem = null;
            ProductCodeInput.Text = string.Empty;
            KeywordInput.Text = string.Empty;

            await SetItems();
        }

        private async void ItemView_Loaded(object sender, RoutedEventArgs e)
        {
            await SetItems();
        }

        private async void ShowNonStockItems_Click(object sender, RoutedEventArgs e)
        {
            isStock = !ShowNonStockItems.IsChecked ?? true;

            await SetItems();
        }

        private static void ToggleVisibility(UIElement element, bool value)
        {
            element.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowBottomBar(bool show = true)
        {
            bottomBarManager.BottomBarHeight = new GridLength(show ? BottomBarManager.DEFAULT_HEIGHT.Value : 0);

            Grid.SetRow(CollapsedButtonBar, show ? 1 : 0);

            bottomBarCollapsed = show;
        }

        private void ShowCancelBtn(bool show = true)
        {
            OrderTabBtn.IsEnabled = !show;
            SettingsBtn.IsEnabled = !show;
            
            var targetBtn = currentWindow == AppPages.INVENTORY ? AddItemBtn : AddOrderBtn;
            ToggleVisibility(targetBtn, !show);

            ToggleVisibility(CancelBtn, show);
        }
        private void InventoryTabBtn_Click(object sender, RoutedEventArgs e)
        {
            currentWindow = AppPages.INVENTORY;

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

        private void OrderTabBtn_Click(object sender, RoutedEventArgs e)
        {
            currentWindow = AppPages.ORDER;

            InventoryTabBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE0E0E0");
            InventoryTabBtn.FontWeight = FontWeights.Normal;

            SettingsBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFE0E0E0");
            SettingsBtn.FontWeight = FontWeights.Normal;

            OrderTabBtn.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF9BE8D6");
            OrderTabBtn.FontWeight = FontWeights.Bold;

            bottomBarManager.CurrentControlContent = new OrderPanel();
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
                }
                else
                {
                    AddOrderCollapsedBtn.Visibility = Visibility.Collapsed;
                    AddItemCollapsedBtn.Visibility = Visibility.Visible;
                }
            }
            else
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

        private void AddOrderCollapsedBtn_Click(object sender, RoutedEventArgs e)
        {
            //CancelNewOrderFormCollapsedBtn.Visibility = Visibility.Visible;
            //AddOrderCollapsedBtn.Visibility = Visibility.Collapsed;

            //ShowAddOrderSection(true);
        }

        private void CancelNewOrderFormCollapsedBtn_Click(object sender, RoutedEventArgs e)
        {
            //CancelNewOrderFormCollapsedBtn.Visibility = Visibility.Collapsed;
            //AddOrderCollapsedBtn.Visibility = Visibility.Visible;

            //ShowAddOrderSection(false);
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            currentWindow = AppPages.SETTINGS;

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

            currentWindow = AppPages.INVENTORY;
        }

        ItemForm itemForm;
        private void ItemView_OnEdit(object sender, ItemEventArgs e)
        {
            itemForm = new ItemForm(
                new ItemFormPresetData(
                    (StockItem)e.Item,
                    enableCategory: false,
                    enableSubcategory: false,
                    enableClass: false
                )
            );

            itemForm.OnConfirmEdit += ItemForm_OnConfirmEdit;
            StartActionWith(itemForm);

            CancelBtn.Click += ItemView_CancelAction;
        }

        private void ItemView_CancelAction(object sender, EventArgs e)
        {
            CancelAction();
            CancelBtn.Click -= ItemView_CancelAction;
        }

        private async void ItemForm_OnConfirmEdit(object sender, ItemEventArgs e)
        {
            await items.UpdateAsync((StockItem)e.OldItem, (StockItem)e.Item);
            CancelAction();
        }

    }
}
