using Dapper;
using System;
using System.Linq;

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

        public async void LoadAsync()
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
}
