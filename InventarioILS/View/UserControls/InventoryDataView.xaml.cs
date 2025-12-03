using System.Collections.Generic;
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
        public static readonly DependencyProperty AutoGenerateColumnsProperty =
            DependencyProperty.Register(
                nameof(AutoGenerateColumns),
                typeof(bool),
                typeof(InventoryDataView),
                new PropertyMetadata(false, OnAutoGenerateColumnsChanged));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable<ItemCard>),
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

        public IEnumerable<ItemCard> ItemsSource
        {
            get => (IEnumerable<ItemCard>)GetValue(ItemsSourceProperty);
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
                view.ItemView.ItemsSource = (IEnumerable<ItemCard>)e.NewValue;
            }
        }
    }
}
