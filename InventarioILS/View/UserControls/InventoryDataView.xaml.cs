using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using Microsoft.Data.Sqlite;

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

        public void SetItemsSource(List<StockItem> items)
        {
            ItemView.ItemsSource = items;
        }
    }
}
