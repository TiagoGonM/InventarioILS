using Microsoft.Data.Sqlite;
using System.IO;

namespace InventarioILS.Model
{
    internal class DbConnection : SqliteConnection
    {
        readonly static string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "inventory.db");

        public DbConnection() : base($"Data Source={dbPath}")
        {
            if (File.Exists(dbPath))
            {
                Open();
            }
            else
            {
                //MessageBox.Show($"Error opening database", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        ~DbConnection()
        {
            if (State == System.Data.ConnectionState.Open)
            {
                Close();
            }
        }      
    }
}
