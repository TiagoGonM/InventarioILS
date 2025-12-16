using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        }

        public async Task AddAsync(ItemMisc item)
        {
            if (Connection == null) return;

            string query = @"INSERT INTO Category (name, shorthand) VALUES (@Name, @Shorthand)";

            var collection = await Connection.ExecuteAsync(query, new
            {
                item.Name,
                item.Shorthand
            }).ConfigureAwait(false);
        }

        public async Task LinkWithAsync(uint subcategoryId, uint? categoryId = null)
        {
            if (Connection == null) return;

            categoryId ??= (uint)LastRowInserted;

            string query = @"INSERT INTO CatSubcat (categoryId, subcategoryId) VALUES (@CatId, @SubcatId)";

            await Connection.ExecuteAsync(query, new {CatId = categoryId, SubcatId = subcategoryId}).ConfigureAwait(false);
        }

        public void Load()
        {
            if (Connection == null) return;

            string query = @$"SELECT categoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Category ORDER BY name ASC";
            var collection = Connection.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            if (Connection == null) return;

            string query = @$"SELECT categoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Category ORDER BY name ASC";
            var collection = await Connection.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }
    }
}
