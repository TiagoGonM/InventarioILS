using Dapper;
using System;
using System.Linq;

namespace InventarioILS.Model.Storage
{
    internal class ItemCategories : SingletonStorage<ItemMisc, ItemCategories>, ILoadSave<ItemMisc>
    {
        public ItemCategories()
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
            string query = @$"SELECT categoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Category ORDER BY name ASC;";
            var collection = Connection.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async void LoadAsync()
        {
            if (Connection == null) return;
            string query = @$"SELECT categoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Category ORDER BY name ASC;";
            var collection = await Connection.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
