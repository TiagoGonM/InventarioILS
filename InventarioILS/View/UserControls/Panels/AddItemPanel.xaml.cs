using InventarioILS.Model;
using InventarioILS.Model.Storage;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InventarioILS.View.UserControls
{
    public partial class AddItemPanel : UserControl, IDisposable
    {
        static readonly StockItems ItemStorage = StockItems.Instance;
        readonly ObservableCollection<StockItem> itemList = [];
        public event Action OnSuccess;

        public StockItem ItemToEdit { get; set; }

        public ItemForm itemForm = null;

        public AddItemPanel()
        {
            InitializeComponent();

            ShowEmptyForm();

            DataContext = new
            {
                ItemList = itemList,
                ItemToEdit,
            };
        }

        private void ShowItemForm(bool show = true)
        {
            FormContainer.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

            ItemCounter.Visibility = !show ? Visibility.Visible : Visibility.Collapsed;
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

            itemForm = new ItemForm(new ItemFormPresetData(item));

            itemForm.OnConfirmEdit += ItemForm_OnEdit;

            FormContainer.Content = itemForm;
            ShowItemForm();
        }

        private void CleanForm()
        {
            FormContainer.Content = null;

            if (itemForm == null) return;

            itemForm.OnConfirm -= ItemForm_OnConfirm;
            itemForm.OnConfirmEdit -= ItemForm_OnEdit;

            itemForm = null;
        }

        private void ItemForm_OnConfirm(object sender, ItemEventArgs e)
        {
            if (e.Item is StockItem item)
            {
                itemList.Add(item);
                ItemCounter.Text = itemList.Count.ToString();
                SetIndexTag(item);
            }

            ShowItemForm(false);
        }

        private void ItemForm_OnEdit(object sender, ItemEventArgs e)
        {
            var item = (StockItem)e.Item;

            int ind = itemList.IndexOf((StockItem)e.OldItem);
            var old = itemList[ind];

            itemList.RemoveAt(ind);
            itemList.Insert(ind, item);

            SetIndexTag(item);

            ShowItemForm(false);
        }

        private void SetIndexTag(StockItem item)
        {
            int ind = itemList.IndexOf(item);

            if (ind == -1) return;
            
            itemList.ElementAt(ind).LocalIndexTag = ind;
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
            await ItemStorage.AddRangeAsync(itemList);
            StatusManager.Instance.UpdateMessageStatusAsync($"Items agregados: {itemList.Count}", Brushes.Green);
            OnSuccess.Invoke();
        }

        private void AddNewItem_Click(object sender, RoutedEventArgs e)
        {
            ShowEmptyForm();
        }

        public void Dispose()
        {
            itemList.Clear();
            OnSuccess = null;
        }
    }
}
