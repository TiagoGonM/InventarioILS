using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InventarioILS.Model
{
    internal class DbConnection
    {
        string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "inventory.db");

        SqliteConnection connection;

        public DbConnection()
        {
            if (File.Exists(dbPath))
            {
                Console.WriteLine("Database file exists.");
            }
            else
            {
                Console.WriteLine("Database file does not exist. Creating a new one.");
            }

           connection = new SqliteConnection($"Data Source={dbPath}");
        }

        //public List<> GetItems()
        //{
        //    var items = connection.Query<StockItemModel>("SELECT it.productCode, c.name category, s.name subcategory, class.name class, it.description, it.createdAt, it.updatedAt \r\nFROM Item it\r\nJOIN Class class ON it.classId = class.classId\r\nJOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId\r\nJOIN Category c ON cs.categoryId = c.categoryId\r\nJOIN Subcategory s ON cs.subcategoryId = s.subcategoryId;").ToList();
        //}
    }
}
