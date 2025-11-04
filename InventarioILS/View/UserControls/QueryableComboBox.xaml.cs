using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InventarioILS.View.UserControls
{
    public partial class QueryableComboBox : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public QueryableComboBox()
        {
            InitializeComponent();
            ComboBox.SelectionChanged += OnComboBoxSelectionChanged;
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
                new PropertyMetadata(null, OnSelectedItem)
            );

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }

        private static void OnSelectedItem(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QueryableComboBox)d;
            control.ComboBox.SelectedItem = (object)e.NewValue;
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnPropertyChanged("SelectionChanged");
        }

        private void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = ComboBox.SelectedItem;
        }

    }
}
