using InventarioILS.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace InventarioILS.View.UserControls
{
    public partial class AddItemPanel : UserControl
    {
        static readonly StockItems ItemsStorage = StockItems.Instance;
        public static ObservableCollection<StockItemExtra> itemList = [];
        public StockItemExtra ItemToEdit { get; set; }

        public AddItemForm itemForm = null;

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

        public static int Count => itemList.Count();

        private void ShowItemForm(bool show = true)
        {
            FormContainer.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            ItemListView.Visibility = !show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowEmptyForm()
        {
            CleanForm();

            itemForm = new AddItemForm();
            itemForm.OnConfirm += ItemForm_OnConfirm;

            FormContainer.Content = itemForm;
            ShowItemForm();
        }

        private void ShowEditForm(StockItemExtra item)
        {
            CleanForm();

            itemForm = new AddItemForm
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

        private void ItemForm_OnConfirm(object sender, StockItemExtraEventArgs e)
        {
            itemList.Add(e.Item);
            SetIndexTag(e.Item);

            ShowItemForm(false);
        }

        private void ItemForm_OnEdit(object sender, StockItemExtraEventArgs e)
        {
            int ind = itemList.IndexOf(e.OldItem);
            var old = itemList[ind];

            itemList.RemoveAt(ind);
            itemList.Insert(ind, e.Item);

            SetIndexTag(e.Item);

            ShowItemForm(false);
        }

        private void SetIndexTag(StockItemExtra item)
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
            ShowEmptyForm();
        }

    }
}
