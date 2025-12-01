using InventarioILS.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls
{
    public partial class AddItemPanel : UserControl
    {
        static readonly StockItems ItemsStorage = StockItems.Instance;
        ObservableCollection<StockItem> itemList = [];

        public AddItemPanel()
        {
            InitializeComponent();

            DataContext = new
            {
                ItemList = itemList,
                itemList.Count
            };            
        }

        private void AddItem_OnConfirm(object sender, StockItemEventArgs e)
        {
            if (itemList.Contains(e.Item) || e.Type != StockItemEventArgs.EventType.CREATE) // Double check validation
                return;

            itemList.Add(e.Item);

            AddItemForm.Visibility = Visibility.Collapsed;
            ItemListView.Visibility = Visibility.Visible;
        }

        private void AddItem_OnEdit(object sender, StockItemEventArgs e)
        {
            if (e.Type != StockItemEventArgs.EventType.EDIT)
                return;

            int ind = itemList.IndexOf(e.OldItem);

            var old = itemList[ind];

            MessageBox.Show($"Old item: {old.ProductCode}\n{old.Description}\n\nItem: {e.Item.ProductCode}\n{e.Item.Description}");

            itemList.RemoveAt(ind);
            itemList.Insert(ind, e.Item);
        }

        private void AddItem_OnDelete(object sender, StockItemEventArgs e)
        {
            if (e.Type != StockItemEventArgs.EventType.DELETE)
                return;

            itemList.Remove(e.Item);
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (itemList.Count == 0) return;

            foreach (var item in itemList)
            {
                for (int i = 0; i < item.Quantity; i++)
                {
                    ItemsStorage.Add(item);
                }
            }
        }

        private void AddNewItem_Click(object sender, RoutedEventArgs e)
        {
            itemList.Add(new StockItem("R-100K", 0, 0, "", 0, 0, "", 10));
        }
    }
}
