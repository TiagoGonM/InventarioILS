using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace InventarioILS
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
            public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string OrdersFileName = "orders.xml";

        private string _orderDescription;
        public string OrderDescription
        {
            get => _orderDescription;
            set { _orderDescription = value; OnPropertyChanged(nameof(OrderDescription)); }
        }

        public ObservableCollection<OrderItem> OrderItems { get; } = new ObservableCollection<OrderItem>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // ejemplo inicial
            OrderItems.Add(new OrderItem { Code = "C-100nF", Category = "Capacitor", Subcategory = "Electrolítico", Description = "Capacitor electrolítico 100nF", Type = "Insumo", Quantity = 5 });

            LoadOrders();
        }

        // Guardar en XML simple (lista)
        private void SaveOrders()
        {
            try
            {
                var list = new List<OrderItem>(OrderItems);
                var serializer = new XmlSerializer(typeof(List<OrderItem>));
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OrdersFileName);
                using (var stream = File.Create(path))
                {
                    serializer.Serialize(stream, list);
                }
                MessageBox.Show("Pedido guardado correctamente.", "Guardar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error guardando: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrders()
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OrdersFileName);
                if (File.Exists(path))
                {
                    var serializer = new XmlSerializer(typeof(List<OrderItem>));
                    using (var stream = File.OpenRead(path))
                    {
                        var list = (List<OrderItem>)serializer.Deserialize(stream);
                        OrderItems.Clear();
                        foreach (var it in list) OrderItems.Add(it);
                    }
                }
            }
            catch
            {
                // si falla, no bloquear UI; se puede loggear si hace falta
            }
        }

        private void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            // asegurar que edits se confirmen antes de guardar
            OrderItemsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            OrderItemsGrid.CommitEdit(DataGridEditingUnit.Row, true);

            // aquí podrías asociar OrderDescription al pedido antes de guardar
            SaveOrders();
        }

        // confirmar edición al salir de celda/fila
        private void OrderItemsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    OrderItemsGrid.CommitEdit(DataGridEditingUnit.Row, true);
                }));
            }
        }

        private void OrderItemsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // forzar actualización de binding
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var binding = (e.Column as DataGridBoundColumn)?.Binding;
                // no hacemos nada extra aquí; CommitEdit en RowEditEnding lo persiste
            }
        }

        private void AddQuantity_Click(object sender, RoutedEventArgs e)
        {
            // ejemplo: incrementar cantidad en la fila correspondiente
            if (sender is Button btn && btn.DataContext is OrderItem item)
            {
                item.Quantity++;
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            // ejemplo simple: limpiar descripción y elementos
            OrderDescription = string.Empty;
            OrderItems.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    [Serializable]
    public class OrderItem : INotifyPropertyChanged
    {
        private string _code;
        private string _category;
        private string _subcategory;
        private string _description;
        private string _type;
        private int _quantity;

        public string Code { get => _code; set { _code = value; OnPropertyChanged(nameof(Code)); } }
        public string Category { get => _category; set { _category = value; OnPropertyChanged(nameof(Category)); } }
        public string Subcategory { get => _subcategory; set { _subcategory = value; OnPropertyChanged(nameof(Subcategory)); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }
        public string Type { get => _type; set { _type = value; OnPropertyChanged(nameof(Type)); } }
        public int Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(nameof(Quantity)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
