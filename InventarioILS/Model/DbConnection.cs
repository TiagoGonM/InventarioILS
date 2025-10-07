using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Windows;

namespace InventarioILS.Model
{
    internal class DbConnection
    {
        readonly string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "inventory.db");

        readonly SqliteConnection connection = null;

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

        ~DbConnection()
        {
            if (connection?.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }

        public List<StockItem> GetStockItems(Dictionary<string, string> filters) {
            
            string query = @"SELECT it.productCode, c.name category, s.name subcategory, class.name class, it.description, st.name state, sto.location, sto.additionalNotes, COUNT(*) quantity
                             FROM ItemStock sto
                             JOIN Item it ON sto.itemId = it.itemId
                             JOIN Class class ON it.classId = class.classId
                             JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                             JOIN Category c ON cs.categoryId = c.categoryId
                             JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                             JOIN State st ON sto.stateId = st.stateId
                             WHERE 1=1";

            var parameters = new DynamicParameters();

            if (filters.ContainsKey("product-code"))
            {
                query += " AND it.productCode LIKE @productCode COLLATE NOCASE";
                parameters.Add("productCode", $"%{filters["product-code"]}%");
            }

            if (filters.ContainsKey("keyword"))
            {
                query += " AND it.description LIKE @keyword COLLATE NOCASE";
                parameters.Add("keyword", $"%{filters["keyword"]}%");
            }
            
            if (filters.ContainsKey("item-class"))
            {
                query += " AND class.name LIKE @className COLLATE NOCASE";
                parameters.Add("className", $"%{filters["item-class"]}%");
            }
            query += @" GROUP BY 
                            it.productCode,
                            c.name,
                            s.name,
                            class.name
                        LIMIT 50;";

            return connection.Query<StockItem>(query, parameters).ToList();
        }

        // TODO: Implement no stock items, try to avoid code duplication and apply DRY
        public List<IItem> GetItems(Dictionary<string, string> filters)
        {
            string query = @"SELECT it.productCode, c.name category, s.name subcategory, class.name class, it.description, it.createdAt, it.updatedAt, COUNT(*) quantity
                             FROM Item it
                             JOIN Class class ON it.classId = class.classId
                             JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                             JOIN Category c ON cs.categoryId = c.categoryId
                             JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                             WHERE 1=1";
            var parameters = new DynamicParameters();

            if (filters.ContainsKey("product-code"))
            {
                query += " AND it.productCode LIKE @productCode COLLATE NOCASE";
                parameters.Add("productCode", $"%{filters["product-code"]}%");
            }

            if (filters.ContainsKey("keyword"))
            {
                query += " AND it.description LIKE @keyword COLLATE NOCASE";
                parameters.Add("keyword", $"%{filters["keyword"]}%");
            }

            if (filters.ContainsKey("item-class"))
            {
                query += " AND class.name LIKE @className COLLATE NOCASE";
                parameters.Add("className", $"%{filters["item-class"]}%");
            }

            query += @" GROUP BY 
                            it.productCode,
                            c.name,
                            s.name,
                            class.name
                        LIMIT 50;";

            return connection.Query<IItem>(@"").ToList();
        }
    }
}
