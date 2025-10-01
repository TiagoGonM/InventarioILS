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

        public List<StockItem> GetStockItems()
        {
            return connection.Query<StockItem>(@"SELECT it.productCode, c.name category, s.name subcategory, class.name class, it.description, st.name state, it.createdAt, it.updatedAt, COUNT(*) quantity
                                                 FROM ItemStock sto
                                                 JOIN Item it ON sto.itemId = it.itemId
                                                 JOIN Class class ON it.classId = class.classId
                                                 JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                                                 JOIN Category c ON cs.categoryId = c.categoryId
                                                 JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                                                 JOIN State st ON sto.stateId = st.stateId
                                                 GROUP BY 
                                                    it.productCode,
                                                    c.name,
                                                    s.name,
                                                    class.name
                                                 LIMIT 50;").ToList();
        }

        public List<StockItem> GetStockItemsByClass(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                return null;
            }

            return connection.Query<StockItem>(@"SELECT it.productCode ProductCode, c.name category, s.name subcategory, class.name Class, it.description Description, st.name State, it.createdAt, it.updatedAt, COUNT(*) Quantity
                                                 FROM ItemStock sto
                                                 JOIN Item it ON sto.itemId = it.itemId
                                                 JOIN Class class ON it.classId = class.classId
                                                 JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                                                 JOIN Category c ON cs.categoryId = c.categoryId
                                                 JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                                                 JOIN State st ON sto.stateId = st.stateId
                                                 WHERE class.name LIKE @ClassName COLLATE NOCASE
                                                 GROUP BY 
                                                    it.productCode,
                                                    c.name,
                                                    s.name,
                                                    class.name
                                                 LIMIT 50;", 
                                                 new { ClassName = $"%{className}%" }).ToList();
        }

        public List<StockItem> GetStockItemsByCode(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                return null;
            }

            // Asegúrate de abrir la conexión si está cerrada
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            // Opción 2: Usar COLLATE NOCASE para comparación insensible a mayúsculas/minúsculas
            return connection.Query<StockItem>(
                @"SELECT it.productCode ProductCode, 
                         c.name category, 
                         s.name subcategory, 
                         class.name Class, 
                         it.description Description, 
                         st.name State, 
                         it.createdAt, 
                         it.updatedAt, 
                         COUNT(*) Quantity
                  FROM ItemStock sto
                  JOIN Item it ON sto.itemId = it.itemId
                  JOIN Class class ON it.classId = class.classId
                  JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                  JOIN Category c ON cs.categoryId = c.categoryId
                  JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                  JOIN State st ON sto.stateId = st.stateId
                  WHERE it.productCode LIKE @ProductCode COLLATE NOCASE
                  GROUP BY 
                     it.productCode,
                     c.name,
                     s.name,
                     class.name
                  LIMIT 50;",
            new { ProductCode = $"%{productCode}%" }).ToList();
        }

        public List<StockItem> GetStockItemsByKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return null;
            }

            return connection.Query<StockItem>(@"SELECT it.productCode ProductCode, c.name category, s.name subcategory, class.name Class, it.description Description, st.name State, it.createdAt, it.updatedAt, COUNT(*) Quantity
                                                 FROM ItemStock sto
                                                 JOIN Item it ON sto.itemId = it.itemId
                                                 JOIN Class class ON it.classId = class.classId
                                                 JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                                                 JOIN Category c ON cs.categoryId = c.categoryId
                                                 JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                                                 JOIN State st ON sto.stateId = st.stateId
                                                 WHERE it.description LIKE @Keyword COLLATE NOCASE
                                                 GROUP BY 
                                                    it.productCode,
                                                    c.name,
                                                    s.name,
                                                    class.name
                                                 LIMIT 50;",
                                                 new { Keyword = $"%{keyword}%" }).ToList();
        }
    }
}
