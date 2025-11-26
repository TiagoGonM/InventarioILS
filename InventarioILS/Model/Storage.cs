using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace InventarioILS.Model
{
    class SQLUtils
    {
        public static string StringCapitalize(string col = "name") =>
            @$"CONCAT(
                UPPER(SUBSTRING({col}, 1, 1)),
                LOWER(SUBSTRING({col}, 2, LENGTH({col})))
            )";
    }
    public interface ILoadSave
    {
        void Add(Item item);
        void Save();
        void Load();
    }

    public class Map<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public void AddOrUpdate(TKey key, TValue value)
        {
            if (!this.ContainsKey(key))
                this.Add(key, value);
            else
                this[key] = value;
        }
    }

    public static class ObservableCollectionExt
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }
    }

    public interface IIdentifiable
    {
        int Id { get; }
    }

    internal class Storage<T> where T: IIdentifiable
    {
        public ObservableCollection<T> Items { get; set; }
        static protected SqliteConnection Connection { get; set; }

        public Storage() 
        {
            Items = [];
            Connection = new DbConnection().Connection;
        }

        protected void UpdateItems(ObservableCollection<T> collection)
        {
            var currentIds = Items.Select(x => x.Id).ToList();
            var newIds = collection.Select(x => x.Id).ToList();

            var itemsToRemove = Items.Where(x => !newIds.Contains(x.Id)).ToList();
            foreach (var item in itemsToRemove)
            {
                Items.Remove(item);
            }

            foreach (var newItem in collection)
            {
                var existingItem = Items.FirstOrDefault(x => x.Id == newItem.Id);
                if (existingItem != null)
                {
                    var index = Items.IndexOf(existingItem);
                    Items[index] = newItem;
                }
                else
                {
                    Items.Add(newItem);
                }
            }
        }
    }

    /// <summary>
    /// Clase base para implementar Singleton sin duplicar código
    /// </summary>
    internal abstract class SingletonStorage<T, TDerived> : Storage<T> 
        where T : IIdentifiable 
        where TDerived : SingletonStorage<T, TDerived>, new()
    {
        private static TDerived _instance;
        private static readonly Lock _lock = new();

        protected SingletonStorage() { }

        /// <summary>
        /// Singleton pattern implementation
        /// </summary>
        public static TDerived Instance
        {
            get
            {
                if (_instance != null) return _instance;
                
                lock (_lock)
                {
                    _instance ??= new TDerived();
                }
                return _instance;
            }
        }
    }

    internal class FiltersImpl<T> where T : Enum
    {
        public Map<T, string> FilterList { get; }

        public FiltersImpl()
        {
            FilterList = [];
        }

        public void AddFilter(T type, string value)
        {
            FilterList.AddOrUpdate(type, value);
        }

        public void RemoveFilter(T type)
        {
            FilterList.Remove(type);
        }

        public void ClearFilters()
        {
            FilterList.Clear();
        }
    }

    internal class ItemCategories : SingletonStorage<Category, ItemCategories>, ILoadSave
    {
        public ItemCategories()
        {
            Load();
        }

        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT categoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Category ORDER BY name ASC;";
            var collection = Connection.Query<Category>(query).ToList().ToObservableCollection();
            UpdateItems(collection);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    internal class ItemSubCategories : SingletonStorage<ItemMisc, ItemSubCategories>, ILoadSave
    {
        public ItemSubCategories()
        {
            Load();
        }

        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT subcategoryId id, {SQLUtils.StringCapitalize()} name FROM Subcategory ORDER BY name ASC;";
            var collection = Connection.Query<ItemMisc>(query).ToList().ToObservableCollection();
            UpdateItems(collection);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    internal class ItemClasses : SingletonStorage<ItemMisc, ItemClasses>, ILoadSave
    {
        public ItemClasses()
        {
            Load();
        }

        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT 
                             classId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM Class ORDER BY name ASC;";
            var collection = Connection.Query<ItemMisc>(query).ToList().ToObservableCollection();
            UpdateItems(collection);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    internal class ItemState : SingletonStorage<ItemMisc, ItemState>, ILoadSave
    {
        public ItemState()
        {
            Load();
        }

        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT 
                             stateId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM State ORDER BY name ASC;";
            var collection = Connection.Query<ItemMisc>(query).ToList().ToObservableCollection();
            UpdateItems(collection);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    internal class StockItems : SingletonStorage<StockItem, StockItems>, ILoadSave
    {
        public enum Filters
        {
            PRODUCT_CODE,
            KEYWORD,
            CLASS_NAME
        }

        public FiltersImpl<Filters> QueryFilters = new();

        public StockItems()
        {
            Load();
        }

        public void Save()
        {
        }

        public void Load()
        {
            if (Connection == null) return;

            string query = @$"SELECT sto.itemStockId id, it.productCode, c.name category, s.name subcategory, {SQLUtils.StringCapitalize("class.name")} class, it.description, {SQLUtils.StringCapitalize("st.name")} state, {SQLUtils.StringCapitalize("sto.location")} location, sto.additionalNotes, COUNT(*) quantity
                                 FROM ItemStock sto
                                 JOIN Item it ON sto.itemId = it.itemId
                                 JOIN Class class ON it.classId = class.classId
                                 JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                                 JOIN Category c ON cs.categoryId = c.categoryId
                                 JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                                 JOIN State st ON sto.stateId = st.stateId
                                 WHERE 1=1";

            var parameters = new DynamicParameters();

            if (QueryFilters.FilterList.ContainsKey(Filters.PRODUCT_CODE))
            {
                query += " AND it.productCode LIKE @productCode COLLATE NOCASE";
                parameters.Add("productCode", $"%{QueryFilters.FilterList[Filters.PRODUCT_CODE]}%");
            }

            if (QueryFilters.FilterList.ContainsKey(Filters.KEYWORD))
            {
                query += " AND it.description LIKE @keyword COLLATE NOCASE";
                parameters.Add("keyword", $"%{QueryFilters.FilterList[Filters.KEYWORD]}%");
            }

            if (QueryFilters.FilterList.ContainsKey(Filters.CLASS_NAME))
            {
                query += " AND class.name LIKE @className COLLATE NOCASE";
                parameters.Add("className", $"%{QueryFilters.FilterList[Filters.CLASS_NAME]}%");
            }

            query += @" GROUP BY 
                            it.productCode,
                            c.name,
                            s.name,
                            class.name
                        LIMIT 50;";

            var collection = Connection.Query<StockItem>(query, parameters).ToList().ToObservableCollection();
            UpdateItems(collection);
        }

        public void Add(Item item)
        {
            if (Connection == null) return;

            // TODO: might be nice to use ids directly instead of names? QueryableComboBox.Tag has these ids if ItemsSource implements IIdentifiable
            //var categoryId = Connection.QuerySingleOrDefault<int>(
            //    "SELECT categoryId FROM Category WHERE name = @CategoryName COLLATE NOCASE",
            //    new { item.CategoryName });

            //var subcategoryId = Connection.QuerySingleOrDefault<int>(
            //    "SELECT subcategoryId FROM Subcategory WHERE name = @SubcategoryName COLLATE NOCASE",
            //    new { item.SubcategoryName });

            ////var classId = Connection.QuerySingleOrDefault<int>

            int catSubcatId = -1;
            
            try
            {
                catSubcatId = Connection.QuerySingleOrDefault<int>(
                    @"SELECT catSubcatId FROM CatSubcat 
                      WHERE categoryId = @CategoryId 
                      AND subcategoryId = @SubcategoryId COLLATE NOCASE",
                    new { item.CategoryId, item.SubcategoryId });
            } catch (Exception ex)
            {
                MessageBox.Show("Failed to retrieve CatSubcatId. " + ex);
                return;
            }


            MessageBox.Show($"CategoryId: {item.CategoryId}, SubcategoryId: {item.SubcategoryId}, CatSubcatId: {catSubcatId}");

            var n = Connection.Execute(
                @"INSERT INTO Item (productCode, catSubcatId, classId, description)
                  VALUES (@ProductCode, @CatSubcatId, @ClassId, @Description)",
                new
                {
                    item.ProductCode,
                    CatSubcatId = catSubcatId,
                    item.ClassId,
                    item.Description,
                }
            );

            if (n <= 0)
            {
                MessageBox.Show("Failed to insert Item record.");
                return;
            }

            if (item is StockItem stockItem)
            {
                Connection.Execute(
                    @"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                      VALUES (@ItemId, @StateId, @Location, @AdditionalNotes)",
                    new
                    {
                        ItemId = Connection.QuerySingle<int>(
                            "SELECT last_insert_rowid()"),
                        stockItem.StateId,
                        stockItem.Location,
                        stockItem.AdditionalNotes
                    }
                );
            }

            Load();
        }
    }

    internal class OrderItems : SingletonStorage<OrderItem, OrderItems>
    {
        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            if (Connection == null) return;

            string query = @"SELECT ordDet.orderDetailId id, o.name, it.productCode, it.description, it.class, ss.name as shipmentState, ordDet.quantity
                             FROM OrderDetail ordDet
                             JOIN 'Order' o ON ordDet.orderId = o.orderId
                             JOIN Item it ON ordDet.itemId = it.itemId
                             JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId;";

            var collection = Connection.Query<OrderItem>(query).ToList().ToObservableCollection();
            UpdateItems(collection);
        }

        public void LoadSingle(int orderId)
        {
            if (Connection == null) return;

            string query = @"SELECT ordDet.orderDetailId id, ord.name, it.productCode, it.description, c.name class, ss.name shipmentState, ordDet.quantity
                             FROM OrderDetail ordDet
                             JOIN 'Order' ord ON ordDet.orderId = ord.orderId
                             JOIN Item it ON ordDet.itemId = it.itemId
                             JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId
                             JOIN Class c ON it.classId = c.classId 
                             WHERE ord.orderId = @OrderId;";

            var collection = Connection.Query<OrderItem>(query, new {OrderId = orderId}).ToList().ToObservableCollection();
            UpdateItems(collection);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    internal class Orders : SingletonStorage<Order, Orders>, ILoadSave
    {
        public enum Filters
        {
            PRODUCT_CODE,
            KEYWORD,
            CLASS_NAME
        }

        public FiltersImpl<Filters> QueryFilters = new();

        public Orders()
        {
            Load();
        }

        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            string query = @"SELECT o.orderId id, o.name, o.description, o.createdAt FROM 'Order' o;";
            var collection = Connection.Query<Order>(query).ToList().ToObservableCollection();
            UpdateItems(collection);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
