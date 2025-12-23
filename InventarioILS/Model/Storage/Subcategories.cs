using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task AddAsync(ItemMisc item)
        {
            using var conn = CreateConnection();

            string query = @"INSERT INTO Subcategory (name, shorthand) VALUES (@Name, @Shorthand)";

            if (string.IsNullOrEmpty(item.Shorthand)) item.Shorthand = null;
            
            await conn.ExecuteAsync(query, new
            {
                item.Name,
                item.Shorthand
            }).ConfigureAwait(false);

            await LoadAsync();
        }

        public void Load()
        {
            using var conn = CreateConnection();
            string query = @$"SELECT subcategoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Subcategory ORDER BY name ASC";

            var collection = conn.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = CreateConnection();
            string query = @$"SELECT subcategoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Subcategory ORDER BY name ASC";

            var collection = await conn.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }
    }
}
