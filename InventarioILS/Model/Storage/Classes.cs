using Dapper;
using System;
using System.Linq;

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

        public void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT 
                             classId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM Class ORDER BY name ASC;";
            var collection = Connection.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async void LoadAsync()
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
}
