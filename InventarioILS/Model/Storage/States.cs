using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    internal class ItemStates : SingletonStorage<ItemMisc, ItemStates>, ILoadSave<ItemMisc>
    {
        public ItemStates()
        {
            Load();
        }

        public void Add(ItemMisc item)
        {
            throw new NotImplementedException();
        }

        public async Task AddAsync(ItemMisc item, uint classId)
        {
            if (Connection == null) return;

            string query = @"INSERT INTO State (name, classId) VALUES (@Name, @ClassId)";
            await Connection.ExecuteAsync(query, new { item.Name, ClassId = classId }).ConfigureAwait(false);
            
            await LoadAsync();
        }

        public void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT 
                             stateId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM State ORDER BY name ASC;";
            var collection = Connection.Query<ItemMisc>(query);

            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            if (Connection == null) return;
            string query = @$"SELECT 
                             stateId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM State ORDER BY name ASC;";
            var collection = await Connection.QueryAsync<ItemMisc>(query).ConfigureAwait(false);

            UpdateItems(collection.ToList().ToObservableCollection());
        }
    }
}
