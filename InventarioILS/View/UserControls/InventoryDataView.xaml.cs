using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para InventoryDataView.xaml
    /// </summary>
    public partial class InventoryDataView : UserControl
    {
        public InventoryDataView()
        {
            InitializeComponent();
        }

        public bool AutoGenerateColumns { set => ItemView.AutoGenerateColumns = value; }
        public IEnumerable<IItem> ItemsSource { set => ItemView.ItemsSource = value; }
        
    }
}
