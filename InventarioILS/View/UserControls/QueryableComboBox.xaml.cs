using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System;

namespace InventarioILS.View.UserControls
{
    public partial class QueryableComboBox : UserControl
    {
        public event EventHandler SelectedItemChanged;

        public QueryableComboBox()
        {
            InitializeComponent();
            ComboBox.SelectionChanged += ComboBox_SelectionChanged;
        }

        public QueryableComboBox(string title, IEnumerable itemsSource, object selectedItem) : this()
        {
            Title = title;
            ItemsSource = itemsSource;
            SelectedItem = selectedItem;
        }

        // Label Text Property
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(QueryableComboBox),
                new PropertyMetadata(String.Empty, OnTitleChanged)
            );

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QueryableComboBox)d;
            control.TitleLabel.Text = e.NewValue.ToString();
        }

        // ItemsSource Property
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(QueryableComboBox),
                new PropertyMetadata(null, OnItemsSourceChanged)
            );

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QueryableComboBox)d;
            control.ComboBox.ItemsSource = (IEnumerable)e.NewValue;
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        // SelectedItem Property
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(QueryableComboBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItem)
            );

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SelectedItem = e.AddedItems[0]; // <- esto actualiza el DP correctamente
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private static void OnSelectedItem(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QueryableComboBox)d;
            control.ComboBox.SelectedItem = (object)e.NewValue;

            control.SelectedItemChanged?.Invoke(control, EventArgs.Empty);
        }
    }
}
