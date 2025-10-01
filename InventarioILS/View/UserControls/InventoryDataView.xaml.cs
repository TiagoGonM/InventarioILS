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
            //DbConnection dbConnection = new DbConnection();
        }

        public void SetItemsSource(List<StockItem> items)
        {
            ItemView.ItemsSource = items;
        }

        private void ItemView_Loaded(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "inventory.db");

            using (var connection = new SqliteConnection($"Data Source={path}"))
            {
                //var items = connection.Query<StockItem>("SELECT it.productCode, c.name category, s.name subcategory, class.name class, it.description, st.name state, it.createdAt, it.updatedAt, COUNT(*) quantity\r\nFROM ItemStock sto\r\nJOIN Item it ON sto.itemId = it.itemId\r\nJOIN Class class ON it.classId = class.classId\r\nJOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId\r\nJOIN Category c ON cs.categoryId = c.categoryId\r\nJOIN Subcategory s ON cs.subcategoryId = s.subcategoryId\r\nJOIN State st ON sto.stateId = st.stateId\r\nGROUP BY \r\n    it.productCode,\r\n    c.name,\r\n    s.name,\r\n    class.name\r\nLIMIT 50;").ToList();
                //ItemView.ItemsSource = items;
            }
        }
    }
}
