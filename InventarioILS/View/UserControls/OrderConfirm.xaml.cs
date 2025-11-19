using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InventarioILS.View.UserControls
{
    public partial class OrderConfirm : UserControl, INotifyPropertyChanged
    {
        private string _orderCode = "[CÓDIGO]";
        public string OrderCode
        {
            get => _orderCode;
            set { _orderCode = value; OnPropertyChanged(nameof(OrderCode)); OnPropertyChanged(nameof(PageIndicator)); }
        }

        private int _currentPage = 1;
        private int _totalPages = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = Math.Max(1, Math.Min(value, TotalPages)); OnPropertyChanged(nameof(CurrentPage)); OnPropertyChanged(nameof(PageIndicator)); }
        }
        public int TotalPages
        {
            get => _totalPages;
            set { _totalPages = Math.Max(1, value); if (CurrentPage > _totalPages) CurrentPage = _totalPages; OnPropertyChanged(nameof(TotalPages)); OnPropertyChanged(nameof(PageIndicator)); }
        }

        public string PageIndicator => $"{CurrentPage}/{TotalPages}";

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = Math.Max(0, value); OnPropertyChanged(nameof(Quantity)); QuantityText = _quantity.ToString(); }
        }

        private string _quantityText = "1";
        public string QuantityText
        {
            get => _quantityText;
            set
            {
                _quantityText = value;
                if (int.TryParse(value, out var v)) _quantity = Math.Max(0, v);
                OnPropertyChanged(nameof(QuantityText));
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public ObservableCollection<string> States { get; } = new ObservableCollection<string>()
        {
            "En Funcionamiento",
            "Dañado",
            "Pendiente",
            "Desconocido"
        };

        private string _selectedState;
        public string SelectedState
        {
            get => _selectedState;
            set { _selectedState = value; OnPropertyChanged(nameof(SelectedState)); }
        }

        private string _location;
        public string Location
        {
            get => _location;
            set { _location = value; OnPropertyChanged(nameof(Location)); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(nameof(Notes)); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public OrderConfirm()
        {
            InitializeComponent();
            DataContext = this;

            SelectedState = States[0];
            Quantity = 1;
            TotalPages = 5; // ejemplo; asigna el total real al abrir
            OrderCode = "[CÓDIGO]";
            Description = "Value";
            Notes = "Value";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            //DialogResult = false;
            //Close();
        }

        private void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuantityText, out var q) || q < 0)
            {
                MessageBox.Show("Introduce una cantidad válida (número positivo).", "Entrada inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // confirmar y cerrar
            //DialogResult = true;
            //Close();
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage > 1) CurrentPage--;
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage < TotalPages) CurrentPage++;
        }

        private static readonly Regex _digitsOnly = new Regex("[^0-9]+");
        private void CantidadTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _digitsOnly.IsMatch(e.Text);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}