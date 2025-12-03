using InventarioILS.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls
{
    public partial class AddItemPanel : UserControl
    {
        static readonly StockItems ItemsStorage = StockItems.Instance;
        public static ObservableCollection<StockItem> itemList = [];
        public StockItem ItemToEdit { get; set; }

        public ItemForm itemForm = null;

        public AddItemPanel()
        {
            InitializeComponent();

            ShowEmptyForm();

            DataContext = new
            {
                ItemList = itemList,
                ItemToEdit
            };
        }

        public static int Count => itemList.Count;

        private void ShowItemForm(bool show = true)
        {
            FormContainer.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            ItemListView.Visibility = !show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowEmptyForm()
        {
            CleanForm();

            itemForm = new ItemForm();
            itemForm.OnConfirm += ItemForm_OnConfirm;

            FormContainer.Content = itemForm;
            ShowItemForm();
        }

        private void ShowEditForm(StockItem item)
        {
            CleanForm();

            itemForm = new ItemForm
            {
                PresetData = item
            };
            itemForm.OnEdit += ItemForm_OnEdit;

            FormContainer.Content = itemForm;
            ShowItemForm();
        }

        private void CleanForm()
        {
            FormContainer.Content = null;

            if (itemForm == null) return;

            itemForm.OnConfirm -= ItemForm_OnConfirm;
            itemForm.OnEdit -= ItemForm_OnEdit;

            itemForm = null;
        }

        private void ItemForm_OnConfirm(object sender, StockItemEventArgs e)
        {
            itemList.Add(e.Item);
            SetIndexTag(e.Item);

            ShowItemForm(false);
        }

        private void ItemForm_OnEdit(object sender, StockItemEventArgs e)
        {
            int ind = itemList.IndexOf(e.OldItem);
            var old = itemList[ind];

            itemList.RemoveAt(ind);
            itemList.Insert(ind, e.Item);

            SetIndexTag(e.Item);

            ShowItemForm(false);
        }

        private void SetIndexTag(StockItem item)
        {
            int ind = itemList.IndexOf(item);
            itemList.ElementAt(ind).Id = ind;
        }

        private void ItemCard_OnEdit(object sender, int e)
        {
            ShowEditForm(itemList.ElementAt(e));
        }

        private void ItemCard_OnDelete(object sender, int e)
        {
            itemList.RemoveAt(e);
        }

        private async void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            await ItemsStorage.AddRangeAsync(itemList);
        }

        private void AddNewItem_Click(object sender, RoutedEventArgs e)
        {
            ShowEmptyForm();
        }

    }
}
