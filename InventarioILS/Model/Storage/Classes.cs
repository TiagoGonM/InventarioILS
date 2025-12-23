using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    internal class ItemClasses : SingletonStorage<ItemMisc, ItemClasses>, ILoadSave<ItemMisc>
    {
        public ItemClasses()
        {
            Load();
        }

        public void Add(ItemMisc item)
        {
            throw new NotImplementedException();
        }

        public async Task AddAsync(ItemMisc item)
        {
            using var conn = CreateConnection();

            string query = @"INSERT INTO Class (name) VALUES (@Name)";
            await conn.ExecuteAsync(query, new { item.Name }).ConfigureAwait(false);
            
            await LoadAsync();
        }

        public void Load()
        {
            using var conn = CreateConnection();

            string query = @$"SELECT 
                             classId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM Class ORDER BY name ASC";
            var collection = conn.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = CreateConnection();

            string query = @$"SELECT 
                             classId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM Class ORDER BY name ASC";
            var collection = await conn.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }
    }
}
