using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.View.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace InventarioILS.View.Windows
{
    /// <summary>
    /// Lógica de interacción para ConfirmReceivedItemsWindow.xaml
    /// </summary>
    public partial class ConfirmReceivedItemsWindow : Window
    {
        readonly ObservableCollection<StockItem> confirmedItems = [];
        readonly ItemStates stateStorage = ItemStates.Instance;
        uint selectedStateId = 0;

        int _currentIndex = 1;
        public int CurrentIndex 
        {
            get => _currentIndex;
            set
            {
                _currentIndex = value;
                CurrentPageIndex.Text = _currentIndex.ToString();
            }
        }
        int TotalPages => items.Count();

        public OrderItem CurrentItem => items.Skip(CurrentIndex - 1).FirstOrDefault();

        readonly IEnumerable<OrderItem> items;

        private void StateComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var item = (ItemMisc)combo.SelectedItem;
            if (item == null) return;

            selectedStateId = item.Id;
        }

        private ConfirmReceivedItemsWindow()
        {
            InitializeComponent();
        }

        public ConfirmReceivedItemsWindow(IEnumerable<OrderItem> items) : this()
        {
            this.items = items;

            TotalItems.Text = TotalPages.ToString();

            NextPageBtn.IsEnabled = TotalPages > 1;
            SubmitBtn.IsEnabled = TotalPages <= 1;

            stateStorage.Load();

            DataContext = new
            {
                StateList = stateStorage.Items,
            };

            FillData();
        }

        private void NextPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex < TotalPages)
            {
                AddNewStockItem(CurrentItem);
                CurrentIndex++;

                PreviousPageBtn.IsEnabled = CurrentIndex > 1;
                SubmitBtn.IsEnabled = CurrentIndex == TotalPages;
                NextPageBtn.IsEnabled = CurrentIndex != TotalPages;
            }

            FillData();
        }


        private void AddOrUpdate(StockItem item)
        {
            if (confirmedItems.Contains(item))
            {
                var existingItem = confirmedItems.FirstOrDefault(i => string.Equals(i.ProductCode, item.ProductCode, StringComparison.OrdinalIgnoreCase));
                confirmedItems.Remove(existingItem);
            }

            confirmedItems.Add(item);
        }

        private void AddNewStockItem(OrderItem item)
        {
            var resultingItem = new StockItem(item, selectedStateId, LocationInput.Text, AdditionalNotesInput.Text);

            AddOrUpdate(resultingItem);
        }

        private void FillData()
        {
            ProductCodeLbl.Text = CurrentItem.ProductCode;
            DescriptionInput.Text = CurrentItem.Description;
            QuantityInput.Text = CurrentItem.Quantity.ToString();
        }

        private void PreviousPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex > 1)
            {
                AddNewStockItem(CurrentItem);
                CurrentIndex--;

                PreviousPageBtn.IsEnabled = CurrentIndex > 1;
            }

            NextPageBtn.IsEnabled = CurrentIndex != TotalPages; 
            FillData();
        }

        private async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            StockItems stockItemStorage = StockItems.Instance;
            OrderItems orderItemStorage = OrderItems.Instance;

            try
            {
                await orderItemStorage.UpdateAsync(items).ConfigureAwait(false);
                await stockItemStorage.AddRangeAsync(confirmedItems).ConfigureAwait(false);
            } catch (Exception ex)
            {
                await StatusManager.Instance.UpdateMessageStatusAsync("Error al intentar confirmar los elementos: " + ex.Message, System.Windows.Media.Brushes.PaleVioletRed);
                throw;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                Close();
            });
        }
    }
}
