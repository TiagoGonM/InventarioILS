using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace InventarioILS.Model
{
    internal class DbConnection : SqliteConnection
    {
        readonly string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "inventory.db");

        public SqliteConnection Connection { get; private set; }

        public DbConnection()
        {
            if (File.Exists(dbPath))
            {
                Connection = new SqliteConnection($"Data Source={dbPath}");
                Connection.Open();
            }
            else
            {
                //MessageBox.Show($"Error opening database", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Connection.Close();
            }
        }

        ~DbConnection()
        {
            if (Connection?.State == System.Data.ConnectionState.Open)
            {
                Connection.Close();
            }
        }      
    }
}
