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

        public static readonly DependencyProperty ProductCodeProperty =
            DependencyProperty.Register(
                nameof(ProductCode),
                typeof(string),
                typeof(ItemCard),
                new PropertyMetadata(string.Empty, OnProductCodeChanged)
            );

        public string ProductCode
        {
            get => (string)GetValue(ProductCodeProperty);
            set => SetValue(ProductCodeProperty, value);
        }

        public static void OnProductCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ItemCard)d;
            control.ProductCode = e.NewValue.ToString();
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
