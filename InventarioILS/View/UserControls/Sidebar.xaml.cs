using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace InventarioILS
{
    /// <summary>
    /// Lógica de interacción para Sidebar.xaml
    /// </summary>
    public partial class Sidebar : UserControl
    {
        public Sidebar()
        {
            InitializeComponent();
        }

        // DependencyProperty que expone el texto (bindable TwoWay)
        public static readonly DependencyProperty ProductCodeTextProperty =
            DependencyProperty.Register(
                name: "ProductCode",
                propertyType: typeof(string),
                ownerType: typeof(Sidebar),
                typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnProductCodeChanged));

        public static readonly DependencyProperty KeywordProperty =
            DependencyProperty.Register(
                name: "Keyword",
                propertyType: typeof(string),
                ownerType: typeof(Sidebar),
                typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string ProductCode
        {
            get => (string)GetValue(ProductCodeTextProperty);
            set => SetValue(ProductCodeTextProperty, value);
        }
        private string Keyword
        {
            get => (string)GetValue(KeywordProperty);
            set => SetValue(KeywordProperty, value);
        }

        private static void OnProductCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Sidebar)d;
            var newValue = e.NewValue as string ?? string.Empty;
            // Sincroniza el TextBox solo si existe y el texto es distinto (evita reentradas)
            if (control.ProductCodeInput != null && control.ProductCodeInput.Text != newValue)
                control.ProductCodeInput.Text = newValue;
        }

        private static void OnKeywordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Sidebar)d;
            var newValue = e.NewValue as string ?? string.Empty;
            // Sincroniza el TextBox solo si existe y el texto es distinto (evita reentradas)
            if (control.KeywordInput != null && control.KeywordInput.Text != newValue)
                control.KeywordInput.Text = newValue;
        }


        // Evento público que reenvía el TextChanged del TextBox interno
        public event TextChangedEventHandler ProductCodeTextChanged;
        public event TextChangedEventHandler KeywordTextChanged;

        private void ProductCodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = ProductCodeInput?.Text ?? string.Empty;

            // Actualiza la DependencyProperty solo si cambió (evita loops)
            if (ProductCode != txt)
                SetCurrentValue(ProductCodeTextProperty, txt);

            // Reenvía evento a suscriptores externos
            ProductCodeTextChanged?.Invoke(this, e);
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void KeywordInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ShowNonStockItems_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddItemBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CollapseSidebarBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearFiltersBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
