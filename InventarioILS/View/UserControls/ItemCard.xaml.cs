using InventarioILS.Model;
using System;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls
{
    public partial class ItemCard : UserControl
    {
        public ItemCard()
        {
            InitializeComponent();
        }

        public event EventHandler<int> OnEdit;
        public event EventHandler<int> OnDelete;

        public static readonly DependencyProperty ItemPresetProperty =
            DependencyProperty.Register(
                nameof(ItemPreset),
                typeof(Item),
                typeof(ItemCard),
                new PropertyMetadata(null, OnPresetItemChanged)
            );

        public Item ItemPreset
        {
            get => (Item)GetValue(ItemPresetProperty);
            set => SetValue(ItemPresetProperty, value);
        }

        public static void OnPresetItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ItemCard)d;
            control.ItemPreset = e.NewValue as Item;
        }

        private void EditItemBtn_Click(object sender, RoutedEventArgs e)
        {
            OnEdit?.Invoke(this, (int)Tag);
        }

        private void RemoveItemBtn_Click(object sender, RoutedEventArgs e)
        {
            OnDelete?.Invoke(this, (int)Tag);
        }
    }
}
