using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections;
using System;
using InventarioILS.Model.Storage;
using System.Linq;

namespace InventarioILS.View.UserControls
{
    public partial class QueryableComboBox : UserControl
    {
        bool _silent = false;

        // Evita que se emita un evento SelectedItemChanged cuando no lo realiza el usuario
        private class SilentScope : IDisposable
        {
            private readonly QueryableComboBox _parent;
            public SilentScope(QueryableComboBox parent) { _parent = parent; _parent._silent = true; }
            public void Dispose() => _parent._silent = false;
        }

        public QueryableComboBox()
        {
            InitializeComponent();
            ComboBox.SelectionChanged += OnInternalSelectionChanged;
        }

        public QueryableComboBox(string title, IEnumerable itemsSource, object selectedItem) : this()
        {
            Title = title;
            ItemsSource = itemsSource;
            SelectedItem = selectedItem;
        }

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
            control.TitleLabel.Visibility = Visibility.Visible;
        }

        public static readonly DependencyProperty ComboTagProperty =
            DependencyProperty.Register(
                "ComboTag",
                typeof(object),
                typeof(QueryableComboBox),
                new PropertyMetadata(null, OnTagChanged)
            );
        public object ComboTag
        {
            get => GetValue(ComboTagProperty);
            set => SetValue(ComboTagProperty, value);
        }

        private static void OnTagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QueryableComboBox)d;
            control.ComboBox.Tag = e.NewValue;
        }


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

        public static readonly RoutedEvent SelectedItemChangedEvent =
            EventManager.RegisterRoutedEvent(
                "SelectedItemChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(QueryableComboBox));

        public event RoutedEventHandler SelectedItemChanged
        {
            add { AddHandler(SelectedItemChangedEvent, value); }
            remove { RemoveHandler(SelectedItemChangedEvent, value); }
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

        // El evento del ComboBox interno
        private void OnInternalSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedItem = ComboBox.SelectedItem;

            if (_silent) return;

            RaiseEvent(new RoutedEventArgs(SelectedItemChangedEvent, this));
        }

        private static void OnSelectedItem(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QueryableComboBox)d;
            
            if (control.ComboBox == null)
                return;
            
            control.ComboBox.SelectedItem = e.NewValue;
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

        private void ComboBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TitleLabel.Foreground = ComboBox.IsEnabled ? (Brush)FindResource("AccentForegroundBrush") : Brushes.Gray;
        }

        public static readonly DependencyProperty SelectedItemIdProperty =
            DependencyProperty.Register(
                nameof(SelectedItemId),
                typeof(uint),
                typeof(QueryableComboBox),
                new PropertyMetadata(0u, OnSelectedItemIdChanged));

        public uint SelectedItemId
        {
            get => (uint)GetValue(SelectedItemIdProperty);
            set => SetValue(SelectedItemIdProperty, value);
        }

        private static void OnSelectedItemIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QueryableComboBox)d;
            uint value = (uint)e.NewValue;

            // Espera a que ItemsSource se haya populado
            control.Dispatcher.InvokeAsync(() =>
            {
                var items = control.ItemsSource?.OfType<IIdentifiable>();
                var item = items?.FirstOrDefault(it => it.Id == value);

                if (item == null) return;

                using (new SilentScope(control))
                {
                    control.SelectedItem = item;
                }
            }, System.Windows.Threading.DispatcherPriority.Loaded
               // debido a que ItemsSource tiene que haberse populado, entonces especificamos que se ejecute luego de que todo haya cargado
               // incluyendo ItemsSource
            );
        }
    }
}
