using Dapper;
using System;
using System.Linq;

namespace InventarioILS.Model.Storage
{
    internal class ItemSubCategories : SingletonStorage<ItemMisc, ItemSubCategories>, ILoadSave<ItemMisc>
    {
        public ItemSubCategories()
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
            string query = @$"SELECT subcategoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Subcategory ORDER BY name ASC;";

            var collection = Connection.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async void LoadAsync()
        {
            if (Connection == null) return;
            string query = @$"SELECT subcategoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Subcategory ORDER BY name ASC;";

            var collection = await Connection.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
