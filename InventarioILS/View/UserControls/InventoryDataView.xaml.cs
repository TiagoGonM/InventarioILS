using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using InventarioILS.Model;

namespace InventarioILS.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para InventoryDataView.xaml
    /// </summary>
    public partial class InventoryDataView : UserControl
    {
        public event EventHandler<ItemEventArgs> OnEdit;

        public static readonly DependencyProperty AutoGenerateColumnsProperty =
            DependencyProperty.Register(
                nameof(AutoGenerateColumns),
                typeof(bool),
                typeof(InventoryDataView),
                new PropertyMetadata(false, OnAutoGenerateColumnsChanged));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable<Item>),
                typeof(InventoryDataView),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public InventoryDataView()
        {
            InitializeComponent();
        }

        public bool AutoGenerateColumns
        {
            get => (bool)GetValue(AutoGenerateColumnsProperty);
            set => SetValue(AutoGenerateColumnsProperty, value);
        }

        public IEnumerable<Item> ItemsSource
        {
            get => (IEnumerable<Item>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void OnAutoGenerateColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InventoryDataView view)
            {
                view.ItemView.AutoGenerateColumns = (bool)e.NewValue;
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InventoryDataView view)
            {
                view.ItemView.ItemsSource = (IEnumerable<Item>)e.NewValue;
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var row = button.DataContext as StockItem;

            OnEdit?.Invoke(this, new ItemEventArgs(row, ItemEventArgs.EventType.EDIT));
        }
    }
}
