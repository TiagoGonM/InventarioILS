using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InventarioILS.View.UserControls
{
    public partial class AddOrderPanel : UserControl
    {
        static readonly OrderItems itemStorage = OrderItems.Instance;
        static readonly ObservableCollection<OrderItem> itemList = [];

        public Action OnSuccess;

        public OrderItem ItemToEdit { get; set; }

        public OrderItemForm itemForm = null;
        
        public AddOrderPanel()
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

            //ItemCounter.Visibility = !show ? Visibility.Visible : Visibility.Collapsed;
            ItemListView.Visibility = !show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowEmptyForm()
        {
            CleanForm();

            itemForm = new OrderItemForm();
            itemForm.OnConfirm += ItemForm_OnConfirm;

            FormContainer.Content = itemForm;
            ShowItemForm();
        }

        private void ShowEditForm(OrderItem item)
        {
            CleanForm();

            itemForm = new OrderItemForm
            {
                PresetData = item
            };
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
            if (e.Item is OrderItem item)
            {
                itemList.Add(item);
                //ItemCounter.Text = itemList.Count.ToString();
                SetIndexTag(item);
            }

            ShowItemForm(false);
        }

        private void ItemForm_OnEdit(object sender, ItemEventArgs e)
        {
            var item = (OrderItem)e.Item;

            int ind = itemList.IndexOf((OrderItem)e.OldItem);
            var old = itemList[ind];

            itemList.RemoveAt(ind);
            itemList.Insert(ind, item);
            //ItemCounter.Text = itemList.Count.ToString();

            SetIndexTag(item);

            ShowItemForm(false);
        }

        // Util para poder determinar que pedido presionó el botón de editar o eliminar seteando el Tag del ItemCard
        private static void SetIndexTag(OrderItem item)
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
            StatusManager.Instance.UpdateMessageStatus($"Pedido agregado: {OrderDescriptionInput.Text} | Items: {itemList.Count}", Brushes.Green);

            await OrderService.RegisterOrder(new Order
            {
                Description = OrderDescriptionInput.Text
            }, itemList);

            OnSuccess.Invoke();
        }

        private void AddNewItem_Click(object sender, RoutedEventArgs e)
        {
            ShowEmptyForm();
        }
    }
}
