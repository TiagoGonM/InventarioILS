using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            set => SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItem(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QueryableComboBox)d;
            
            if (control.ComboBox == null)
                return;

            control.ComboBox.SelectedItem = (object)e.NewValue;
            control.SelectedItemChanged?.Invoke(control, EventArgs.Empty);
        }

        // Background Property
        public static readonly DependencyProperty ComboBoxBackgroundProperty =
            DependencyProperty.Register(
                nameof(ComboBoxBackground),
                typeof(Brush),
                typeof(QueryableComboBox),
                new PropertyMetadata(null, OnComboBoxBackgroundChanged)
            );

        public Brush ComboBoxBackground
        {
            get => (Brush)GetValue(ComboBoxBackgroundProperty);
            set => SetValue(ComboBoxBackgroundProperty, value);
        }

        private static void OnComboBoxBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QueryableComboBox)d;
            if (control.ComboBox != null)
                control.ComboBox.Background = (Brush)e.NewValue;
        }
    }
}
