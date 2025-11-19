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

        
        List<OrderItem> GetItems(Dictionary<string, string> filters)
        {
            if (Connection == null) return new List<OrderItem>();

            string query = @"SELECT it.productCode, c.name category, s.name subcategory, class.name class, it.description, it.createdAt, it.updatedAt, COUNT(*) quantity
                             FROM Item it
                             JOIN Class class ON it.classId = class.classId
                             JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                             JOIN Category c ON cs.categoryId = c.categoryId
                             JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                             WHERE 1=1";

            var parameters = new DynamicParameters();

            if (filters.ContainsKey("productCode"))
            {
                query += " AND it.productCode LIKE @productCode COLLATE NOCASE";
                parameters.Add("productCode", $"%{filters["productCode"]}%");
            }

            if (filters.ContainsKey("keyword"))
            {
                query += " AND it.description LIKE @keyword COLLATE NOCASE";
                parameters.Add("keyword", $"%{filters["keyword"]}%");
            }

            if (filters.ContainsKey("className"))
            {
                query += " AND class.name LIKE @className COLLATE NOCASE";
                parameters.Add("className", $"%{filters["className"]}%");
            }

            query += @" GROUP BY 
                            it.productCode,
                            c.name,
                            s.name,
                            class.name
                        LIMIT 50;";

            return Connection.Query<OrderItem>(query, parameters).ToList();
        }

        void SaveItem(Item item)
        {
            if (Connection == null) return;

            var categoryId = Connection.QuerySingleOrDefault<int>(
                "SELECT categoryId FROM Category WHERE name = @CategoryName COLLATE NOCASE",
                new { item.CategoryId });

            var subcategoryId = Connection.QuerySingleOrDefault<int>(
                "SELECT subcategoryId FROM Subcategory WHERE name = @SubcategoryName COLLATE NOCASE",
                new { item.SubcategoryId });

            var catSubcatId = Connection.QuerySingleOrDefault<int>(
                @"SELECT catSubcatId FROM CatSubcat 
          WHERE categoryId = @categoryId AND subcategoryId = @subcategoryId COLLATE NOCASE",
                new { categoryId, subcategoryId });

            MessageBox.Show($"CategoryId: {categoryId}, SubcategoryId: {subcategoryId}, CatSubcatId: {catSubcatId}");

            // Si no existe la relación, crearla
            //if (catSubcatId == 0)
            //{
            //    connection.Execute(
            //        "INSERT INTO CatSubcat (categoryId, subcategoryId) VALUES (@categoryId, @subcategoryId)",
            //        new { categoryId, subcategoryId });

            //    catSubcatId = connection.QuerySingle<int>(
            //        "SELECT last_insert_rowid()");
            //}

            Connection.Execute(@"INSERT INTO Item (productCode, catSubcatId, classId, description)
                                 VALUES (@ProductCode, @CatSubcatId, @ClassId, @Description)",
            new {
                item.ProductCode,
                CatSubcatId = catSubcatId,
                ClassId = item.ClassId,
                item.Description,
            });

            if (item is StockItem stockItem)
            {
                var stateId = Connection.QuerySingleOrDefault<int>(
                    "SELECT stateId FROM State WHERE name = @State COLLATE NOCASE",
                    new { stockItem.StateId });

                Connection.Execute(@"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                                     VALUES (@ItemId, @StateId, @Location, @AdditionalNotes)",
                new {
                    ItemId = Connection.QuerySingle<int>(
                        "SELECT last_insert_rowid()"),
                    StateId = stateId,
                    stockItem.Location,
                    stockItem.AdditionalNotes
                });
            } 
        }
        
    }
}
