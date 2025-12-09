using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace InventarioILS.Model.Storage
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
            if (!ContainsKey(key))
                Add(key, value);
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
        uint? Id { get; }
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

    public class Utils
    {
        public static int AddItem(Item item)
        {
            var conn = new DbConnection();

            if (conn == null) return -1;

            int catSubcatId = -1;

            try
            {
                catSubcatId = conn.QuerySingleOrDefault<int>(
                    @"SELECT catSubcatId FROM CatSubcat 
                      WHERE categoryId = @CategoryId 
                      AND subcategoryId = @SubcategoryId COLLATE NOCASE",
                    new { item.CategoryId, item.SubcategoryId });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to retrieve CatSubcatId. " + ex);
                return -1;
            }

            //MessageBox.Show($"CategoryId: {item.CategoryId}, SubcategoryId: {item.SubcategoryId}, CatSubcatId: {catSubcatId}");

            var n = conn.Execute(
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
                return -1;
            }

            int rowid = conn.QuerySingle<int>("SELECT last_insert_rowid()");
            return rowid;
        }

        /// <returns>number of the row that got inserted, -1 if something goes wrong</returns>
        public async static Task<int> AddItemAsync(Item item)
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


        public async static Task<int> AddItemAsync(Item item, IDbTransaction transaction)
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
}
