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

        // TODO: i don't like what i did here
        public class DynamicQueryBuilder
        {
            readonly DynamicParameters parameters;

            public string Query { get; private set; }

            public DynamicQueryBuilder(string mainQuery)
            {
                parameters = new DynamicParameters();
                Query = mainQuery;
            }

            public void AddFilter(string tableColumn, string paramName, string filterValue)
            {
                Query += $" AND {tableColumn} LIKE @{paramName} COLLATE NOCASE";
                parameters.Add(paramName, $"%{filterValue}%");
            }

            public (string, DynamicParameters) GetQueryTuple()
            {
                return (Query, parameters);
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

            var queryObj = new DynamicQueryBuilder(query);

            if (filters.ContainsKey("productCode")) queryObj.AddFilter("it.productCode", "productCode", filters["productCode"]);
            if (filters.ContainsKey("keyword")) queryObj.AddFilter("it.description", "keyword", filters["keyword"]);
            if (filters.ContainsKey("className")) queryObj.AddFilter("class.name", "className", filters["className"]);

            (string sql, var parameters) = queryObj.GetQueryTuple();

            sql += @" GROUP BY 
                            it.productCode,
                            c.name,
                            s.name,
                            class.name
                        LIMIT 50;";

            return connection.Query<StockItem>(sql, parameters).ToList();
        }

        // TODO: Implement no stock items, try to avoid code duplication and apply DRY
        public List<OrderItem> GetItems(Dictionary<string, string> filters)
        {
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

            return connection.Query<OrderItem>(query, parameters).ToList();
        }
    }
}
