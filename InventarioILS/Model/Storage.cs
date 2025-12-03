using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

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
        int? Id { get; }
    }

    internal class Storage<T> where T: IIdentifiable
    {
        public ObservableCollection<T> Items { get; set; }
        static public DbConnection Connection { get; set; }

        public Storage()
        {
            Items = [];
            Connection = new DbConnection();
        }

        protected void UpdateItems(ObservableCollection<T> collection)
        {
            if (Connection == null) return;

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

        protected async Task UpdateItemsAsync(ObservableCollection<T> collection)
        {
            var dispatcher = Application.Current?.Dispatcher;

            // Verifica que la aplicación no se haya cerrado de manera inesperada
            if (dispatcher == null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
            {
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateItems(collection.ToList().ToObservableCollection());
            });
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

        public async void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT categoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Category ORDER BY name ASC;";
            var collection = await Connection.QueryAsync<Category>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
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

        public async void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT subcategoryId id, {SQLUtils.StringCapitalize()} name FROM Subcategory ORDER BY name ASC;";

            var collection = await Connection.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
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

        public async void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT 
                             classId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM Class ORDER BY name ASC;";
            var collection = await Connection.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    internal class ItemStates : SingletonStorage<ItemMisc, ItemStates>, ILoadSave
    {
        public ItemStates()
        {
            Load();
        }

        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public async void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT 
                             stateId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM State ORDER BY name ASC;";
            var collection = await Connection.QueryAsync<ItemMisc>(query).ConfigureAwait(false);

            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    public class Utils
    {
        /// <returns>number of the row that got inserted, -1 if something goes wrong</returns>
        public async static Task<int> AddItem(Item item)
        {
            var conn = new DbConnection();

            if (conn == null) return -1;

            int catSubcatId = -1;

            try
            {
                catSubcatId = await conn.QuerySingleOrDefaultAsync<int>(
                    @"SELECT catSubcatId FROM CatSubcat 
                      WHERE categoryId = @CategoryId 
                      AND subcategoryId = @SubcategoryId COLLATE NOCASE",
                    new { item.CategoryId, item.SubcategoryId }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to retrieve CatSubcatId. " + ex);
                return -1;
            }

            //MessageBox.Show($"CategoryId: {item.CategoryId}, SubcategoryId: {item.SubcategoryId}, CatSubcatId: {catSubcatId}");

            var n = await conn.ExecuteAsync(
                @"INSERT INTO Item (productCode, catSubcatId, classId, description)
                  VALUES (@ProductCode, @CatSubcatId, @ClassId, @Description)",
                new
                {
                    item.ProductCode,
                    CatSubcatId = catSubcatId,
                    item.ClassId,
                    item.Description,
                }
            ).ConfigureAwait(false);

            if (n <= 0)
            {
                MessageBox.Show("Failed to insert Item record.");
                return -1;
            }

            int rowid = await conn.QuerySingleAsync<int>("SELECT last_insert_rowid()").ConfigureAwait(false);
            return rowid;
        }

        public async static Task<int> AddItem(Item item, IDbTransaction transaction)
        {
            var conn = transaction.Connection ?? throw new InvalidOperationException("La conexión de la transacción es nula.");
            
            int catSubcatId;

            try
            {
                catSubcatId = await conn.QuerySingleOrDefaultAsync<int>(
                    @"SELECT catSubcatId FROM CatSubcat
                      WHERE categoryId = @CategoryId 
                      AND subcategoryId = @SubcategoryId COLLATE NOCASE",
                    new { item.CategoryId, item.SubcategoryId },
                    transaction: transaction
                ).ConfigureAwait(false);

                if (catSubcatId == default) // Si no se encontró el ID
                {
                    throw new InvalidOperationException(
                        $"No se encontró CatSubcatId para CategoryId: {item.CategoryId}, SubcategoryId: {item.SubcategoryId}");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al buscar CatSubcatId.", ex);
            }

            string insertSql =
                @"INSERT INTO Item (productCode, catSubcatId, classId, description)
                  VALUES (@ProductCode, @CatSubcatId, @ClassId, @Description);
                  SELECT last_insert_rowid();";

            var parameters = new
            {
                item.ProductCode,
                CatSubcatId = catSubcatId,
                item.ClassId,
                item.Description,
            };

            int rowId = await conn.ExecuteScalarAsync<int>(
                insertSql,
                parameters,
                transaction: transaction
            ).ConfigureAwait(false);

            return rowId >= 0 ? rowId : -1;
        }
    }

    internal class StockItems : SingletonStorage<StockItem, StockItems>
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

            if (QueryFilters.FilterList.TryGetValue(Filters.PRODUCT_CODE, out string productCode))
            {
                query += " AND it.productCode LIKE @productCode COLLATE NOCASE";
                parameters.Add("productCode", $"%{productCode}%");
            }

            if (QueryFilters.FilterList.TryGetValue(Filters.KEYWORD, out string keyword))
            {
                query += " AND it.description LIKE @keyword COLLATE NOCASE";
                parameters.Add("keyword", $"%{keyword}%");
            }

            if (QueryFilters.FilterList.TryGetValue(Filters.CLASS_NAME, out string className))
            {
                query += " AND class.name LIKE @className COLLATE NOCASE";
                parameters.Add("className", $"%{className}%");
            }

            query += @" GROUP BY 
                            it.productCode,
                            c.name,
                            s.name,
                            class.name
                        LIMIT 50;";

            var collection = Connection.Query<StockItem>(query, parameters);

            UpdateItems(collection.ToList().ToObservableCollection());
        }

        // TODO: finish this
        public async Task LoadAsync()
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

            if (QueryFilters.FilterList.TryGetValue(Filters.PRODUCT_CODE, out string productCode))
            {
                query += " AND it.productCode LIKE @productCode COLLATE NOCASE";
                parameters.Add("productCode", $"%{productCode}%");
            }

            if (QueryFilters.FilterList.TryGetValue(Filters.KEYWORD, out string keyword))
            {
                query += " AND it.description LIKE @keyword COLLATE NOCASE";
                parameters.Add("keyword", $"%{keyword}%");
            }

            if (QueryFilters.FilterList.TryGetValue(Filters.CLASS_NAME, out string className))
            {
                query += " AND class.name LIKE @className COLLATE NOCASE";
                parameters.Add("className", $"%{className}%");
            }

            query += @" GROUP BY 
                            it.productCode,
                            c.name,
                            s.name,
                            class.name
                        LIMIT 50;";

            var collection = await Connection.QueryAsync<StockItem>(query, parameters).ConfigureAwait(false);

            await UpdateItemsAsync(collection.ToList().ToObservableCollection()).ConfigureAwait(false);
        }

        public async Task Add(Item item)
        {
            int rowId = await Utils.AddItem(item);

            if (item is StockItem stockItem)
            {
                await Connection.ExecuteAsync(
                    @"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                      VALUES (@ItemId, @StateId, @Location, @AdditionalNotes)",
                    new
                    {
                        ItemId = rowId,
                        stockItem.StateId,
                        stockItem.Location,
                        stockItem.AdditionalNotes
                    }
                ).ConfigureAwait(false);
            }

            Load();
        }

        public void AddAsync(Item item)
        {
            int rowId = Utils.AddItem(item);

            if (item is StockItem stockItem)
            {
                Connection.Execute(
                    @"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                      VALUES (@ItemId, @StateId, @Location, @AdditionalNotes)",
                    new
                    {
                        ItemId = rowId,
                        stockItem.StateId,
                        stockItem.Location,
                        stockItem.AdditionalNotes
                    }
                );
            }

            Load();
        }

        public void AddRange(ObservableCollection<StockItem> collection)
        {
            if (collection.Count == 0) return;

            foreach (var item in collection)
            {
                for (int i = 0; i < item.Quantity; i++)
                {
                    Add(item);
                }
            }
        }

        public async Task AddRangeAsync(ObservableCollection<StockItem> collection)
        {
            if (collection.Count == 0) return;

            await Task.Run(async () =>
            {
                foreach (var item in collection)
                {
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        await Add(item);
                    }
                }
            });
        }

        public async Task AddRangeAsyncTransact(ObservableCollection<StockItem> items)
        {
            await Task.Run(async () =>
            {
                // 1. Iniciar una ÚNICA Conexión y Transacción para el lote.
                // Asumiendo que 'Connection' es tu DbConnection.
                using (var transaction = Connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in items)
                        {
                            // Nota: Aquí no necesitamos iterar por item.Quantity si ya aplanamos la lista
                            // en AddRangeAsync para que 'items' ya contenga una fila por unidad.

                            // A. Insertar en Item y obtener el ID.
                            //    'Utils.AddItem' debe aceptar la transacción y devolver el nuevo ID.
                            int itemId = await Utils.AddItem(item, transaction).ConfigureAwait(false);

                            // B. Insertar en ItemStock usando el ID generado.
                            await Connection.ExecuteAsync(
                                @"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                          VALUES (@ItemId, @StateId, @Location, @AdditionalNotes)",
                                new
                                {
                                    ItemId = itemId,
                                    item.StateId,
                                    item.Location,
                                    item.AdditionalNotes
                                },
                                transaction: transaction // 👈 ¡CLAVE! Dapper ejecuta dentro de la transacción.
                            ).ConfigureAwait(false);
                        }

                        // 2. Si todo fue bien, confirmar una vez.
                        transaction.Commit();
                    }
                    catch
                    {
                        // 3. Si hay un fallo, revertir todo el lote.
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }
    }

    internal class OrderItems : SingletonStorage<OrderItem, OrderItems>
    {
        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public async void Load()
        {
            if (Connection == null) return;

            string query = @"SELECT ordDet.orderDetailId id, o.name, it.productCode, it.description, it.class, ss.name as shipmentState, ordDet.quantity
                             FROM OrderDetail ordDet
                             JOIN 'Order' o ON ordDet.orderId = o.orderId
                             JOIN Item it ON ordDet.itemId = it.itemId
                             JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId;";

            var collection = await Connection.QueryAsync<OrderItem>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async void LoadSingle(int orderId)
        {
            if (Connection == null) return;

            string query = @"SELECT ordDet.orderDetailId id, ord.name, it.productCode, it.description, c.name class, ss.name shipmentState, ordDet.quantity
                             FROM OrderDetail ordDet
                             JOIN 'Order' ord ON ordDet.orderId = ord.orderId
                             JOIN Item it ON ordDet.itemId = it.itemId
                             JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId
                             JOIN Class c ON it.classId = c.classId 
                             WHERE ord.orderId = @OrderId;";

            var collection = await Connection.QueryAsync<OrderItem>(query, new {OrderId = orderId}).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async void Save()
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

        public async void Load()
        {
            string query = @"SELECT o.orderId id, o.name, o.description, o.createdAt FROM 'Order' o;";
            var collection = await Connection.QueryAsync<Order>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
