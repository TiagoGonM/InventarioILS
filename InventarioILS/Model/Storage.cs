using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace InventarioILS.Model
{
    public interface ILoadSave
    {
        void Add(Item item);
        void Save();
        void Load();
    }

    public enum Filters
    {
        PRODUCT_CODE,
        KEYWORD,
        CLASS_NAME
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

            // Eliminar items que ya no existen en la nueva colección
            var itemsToRemove = Items.Where(x => !newIds.Contains(x.Id)).ToList();
            foreach (var item in itemsToRemove)
            {
                Items.Remove(item);
            }

            // Actualizar items existentes y agregar nuevos
            foreach (var newItem in collection)
            {
                var existingItem = Items.FirstOrDefault(x => x.Id == newItem.Id);
                if (existingItem != null)
                {
                    // Actualizar el item existente con los nuevos datos
                    var index = Items.IndexOf(existingItem);
                    Items[index] = newItem;
                }
                else
                {
                    // Agregar nuevo item
                    Items.Add(newItem);
                }
            }
        }
    }
    internal class ItemCategories : Storage<ItemMisc>, ILoadSave
    {
        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            if (Connection == null) return;
            string query = @"SELECT categoryId id, name FROM Category;";
            var collection = Connection.Query<ItemMisc>(query).ToList().ToObservableCollection();

            UpdateItems(collection);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    internal class ItemSubCategories : Storage<ItemMisc>, ILoadSave
    {
        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            if (Connection == null) return;
            string query = @"SELECT subcategoryId id, name FROM Subcategory;";
            var collection = Connection.Query<ItemMisc>(query).ToList().ToObservableCollection();

            UpdateItems(collection);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    internal class ItemClasses : Storage<ItemMisc>, ILoadSave
    {
        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            if (Connection == null) return;
            string query = @"SELECT classId id, name FROM Class;";
            var collection = Connection.Query<ItemMisc>(query).ToList().ToObservableCollection();

            UpdateItems(collection);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    internal class StockItems : Storage<StockItem>, ILoadSave
    {
        Map<Filters, string> QueryFilters { get; }

        public StockItems() : base()
        {
            QueryFilters = [];
        }

        public void AddFilter(Filters type, string value)
        {
            QueryFilters.AddOrUpdate(type, value);
        }

        public void RemoveFilter(Filters type)
        {
            QueryFilters.Remove(type);
        }

        public void ClearFilters()
        {
            QueryFilters.Clear();
        }

        public void Save()
        {
        }

        public void Load()
        {
            if (Connection == null) return;

            string query = @"SELECT sto.itemStockId id, it.productCode, c.name category, s.name subcategory, class.name class, it.description, st.name state, sto.location, sto.additionalNotes, COUNT(*) quantity
                                 FROM ItemStock sto
                                 JOIN Item it ON sto.itemId = it.itemId
                                 JOIN Class class ON it.classId = class.classId
                                 JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                                 JOIN Category c ON cs.categoryId = c.categoryId
                                 JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                                 JOIN State st ON sto.stateId = st.stateId
                                 WHERE 1=1";

            var parameters = new DynamicParameters();

            if (QueryFilters.ContainsKey(Filters.PRODUCT_CODE))
            {
                query += " AND it.productCode LIKE @productCode COLLATE NOCASE";
                parameters.Add("productCode", $"%{QueryFilters[Filters.PRODUCT_CODE]}%");
            }

            if (QueryFilters.ContainsKey(Filters.KEYWORD))
            {
                query += " AND it.description LIKE @keyword COLLATE NOCASE";
                parameters.Add("keyword", $"%{QueryFilters[Filters.KEYWORD]}%");
            }

            if (QueryFilters.ContainsKey(Filters.CLASS_NAME))
            {
                query += " AND class.name LIKE @className COLLATE NOCASE";
                parameters.Add("className", $"%{QueryFilters[Filters.CLASS_NAME]}%");
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

            var categoryId = Connection.QuerySingleOrDefault<int>(
                "SELECT categoryId FROM Category WHERE name = @CategoryName COLLATE NOCASE",
                new { item.CategoryName });

            var subcategoryId = Connection.QuerySingleOrDefault<int>(
                "SELECT subcategoryId FROM Subcategory WHERE name = @SubcategoryName COLLATE NOCASE",
                new { item.SubcategoryName });

            var catSubcatId = Connection.QuerySingleOrDefault<int>(
                @"SELECT catSubcatId FROM CatSubcat 
                  WHERE categoryId = @categoryId 
                  AND subcategoryId = @subcategoryId COLLATE NOCASE",
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
            new
            {
                item.ProductCode,
                CatSubcatId = catSubcatId,
                ClassId = item.Class,
                item.Description,
            });

            if (item is StockItem stockItem)
            {
                var stateId = Connection.QuerySingleOrDefault<int>(
                    "SELECT stateId FROM State WHERE name = @State COLLATE NOCASE",
                    new { stockItem.State });

                Connection.Execute(@"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                                     VALUES (@ItemId, @StateId, @Location, @AdditionalNotes)",
                new
                {
                    ItemId = Connection.QuerySingle<int>(
                        "SELECT last_insert_rowid()"),
                    StateId = stateId,
                    stockItem.Location,
                    stockItem.AdditionalNotes
                });
            }
        }
    }

    internal class OrderItems : Storage<OrderItem>, ILoadSave
    {
        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    internal class Orders : Storage<Order>, ILoadSave
    {
        Map<Filters, string> QueryFilters { get; }

        public Orders() : base()
        {
            QueryFilters = [];
        }

        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            string query = @"SELECT o.orderId id, o.name, o.description, o.createdAt FROM 'Order' o;";

            var collection = Connection.Query<Order>(query).ToList().ToObservableCollection();

            //foreach (var obj in collection)
            //{
            //    MessageBox.Show($"Order ID: {obj.Id}, Name: {obj.Name}, Description: {obj.Description}, Created At: {obj.CreatedAt}");
            //}

            UpdateItems(collection);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

}
