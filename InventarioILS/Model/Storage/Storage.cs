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

        public static string IncludeLastRowIdInserted(string sql) =>
            sql + "; SELECT last_insert_rowid();";
    }

    public interface ILoadSave<T>
    {
        void Add(T item);
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
        uint Id { get; }
    }

    public class Storage<T> where T: IIdentifiable
    {
        public ObservableCollection<T> Items { get; set; }

        public static DbConnection CreateConnection() => DbConnection.CreateAndOpen();
        public static async Task<DbConnection> CreateConnectionAsync() => await DbConnection.CreateAndOpenAsync();

        public Storage()
        {
            Items = [];
        }

        protected void UpdateItems(ObservableCollection<T> collection)
        {
            // 1. Creamos el mapa ignorando duplicados (o quedándonos con el último)
            // Esto agiliza la búsqueda sin riesgo de "Duplicate Key Exception"
            var existingItemsMap = new Dictionary<uint, T>();
            foreach (var item in Items)
            {
                existingItemsMap[item.Id] = item; // Si el ID se repite, simplemente se sobreescribe
            }

            // 2. IDs de la nueva colección para saaber qué borrar
            var newIds = new HashSet<uint>(collection.Select(item => (uint)item.Id));

            // 3. Eliminación eficiente (de atrás hacia adelante)
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (!newIds.Contains((uint)Items[i].Id))
                {
                    Items.RemoveAt(i);
                }
            }

            // 4. Actualización o Inserción
            foreach (var newItem in collection)
            {
                if (existingItemsMap.TryGetValue(newItem.Id, out T existingItem))
                {
                    // Opcional: Si el objeto es exactamente la misma instancia, 
                    // no hace falta remover y agregar.
                    if (ReferenceEquals(existingItem, newItem)) continue;

                    Items.Remove(existingItem);
                }

                Items.Add(newItem);
            }
        }

        //protected void UpdateItems(ObservableCollection<T> collection)
        //{
        //    var existingItemsMap = Items.ToDictionary(item => item.Id, item => item); // FIXME

        //    var newIds = new HashSet<uint>(collection.Select(item => (uint)item.Id));

        //    for (int i = Items.Count - 1; i >= 0; i--)
        //    {
        //        var existingItem = Items[i];

        //        if (!newIds.Contains((uint)existingItem.Id))
        //        {
        //            Items.RemoveAt(i); // RemoveAt es más eficiente que Items.Remove(item)
        //        }
        //    }

        //    foreach (var newItem in collection)
        //    {
        //        if (existingItemsMap.TryGetValue(newItem.Id, out T existingItem))
        //        {
        //            Items.Remove(existingItem);
        //        }
                
        //        Items.Add(newItem);
        //    }
        //}

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

    public abstract class SingletonStorage<T, TDerived> : Storage<T>
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

    public class FiltersImpl<T> where T : Enum
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
}
