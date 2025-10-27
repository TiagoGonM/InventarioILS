using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static InventarioILS.Model.DbConnection;

namespace InventarioILS.Model
{
    interface ILoadSave
    {
        void SaveItem(Item item);

        void LoadItem(Item item);
    }

    internal class ItemStorage
    {
        ObservableCollection<Item> Items { get; }
        DbConnection Connection { get; }

        public ItemStorage() 
        {
            Items = new ObservableCollection<Item>();
            Connection = new DbConnection();
        }
    }

    internal class StockItems : ItemStorage, ILoadSave
    {
        public void SaveItem(Item item)
        {
        }

        public void LoadItem(Item item) 
        {
            //    //if (Connection == null) return new List<StockItem>();

            //    string query = @"SELECT it.productCode, c.name category, s.name subcategory, class.name class, it.description, st.name state, sto.location, sto.additionalNotes, COUNT(*) quantity
            //                     FROM ItemStock sto
            //                     JOIN Item it ON sto.itemId = it.itemId
            //                     JOIN Class class ON it.classId = class.classId
            //                     JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
            //                     JOIN Category c ON cs.categoryId = c.categoryId
            //                     JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
            //                     JOIN State st ON sto.stateId = st.stateId
            //                     WHERE 1=1";

            //    //var parameters = new DynamicParameters();

            //    //if (filters.ContainsKey("productCode"))
            //    //{
            //    //    query += " AND it.productCode LIKE @productCode COLLATE NOCASE";
            //    //    parameters.Add("productCode", $"%{filters["productCode"]}%");
            //    //}

            //    //if (filters.ContainsKey("keyword"))
            //    //{
            //    //    query += " AND it.description LIKE @keyword COLLATE NOCASE";
            //    //    parameters.Add("keyword", $"%{filters["keyword"]}%");
            //    //}

            //    //if (filters.ContainsKey("className"))
            //    //{
            //    //    query += " AND class.name LIKE @className COLLATE NOCASE";
            //    //    parameters.Add("className", $"%{filters["className"]}%");
            //    //}

            //    query += @" GROUP BY 
            //                    it.productCode,
            //                    c.name,
            //                    s.name,
            //                    class.name
            //                LIMIT 50;";

            //    return Connection.Query<StockItem>(query, parameters).ToList();
        }
    }

    internal class OrderItems : ItemStorage, ILoadSave
    {
        public void LoadItem(Item item)
        {
            throw new NotImplementedException();
        }

        public void SaveItem(Item item)
        {
            throw new NotImplementedException();
        }
    }
}
