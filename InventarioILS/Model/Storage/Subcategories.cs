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
            if (Connection == null) return;

            string query = @"INSERT INTO Subcategory (name, shorthand) VALUES (@Name, @Shorthand)";

            if (string.IsNullOrEmpty(item.Shorthand)) item.Shorthand = null;
            
            await Connection.ExecuteAsync(query, new
            {
                item.Name,
                item.Shorthand
            }).ConfigureAwait(false);

            await LoadAsync();
        }

        public void Load()
        {
            if (Connection == null) return;
            string query = @$"SELECT subcategoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Subcategory ORDER BY name ASC;";

            var collection = Connection.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            if (Connection == null) return;
            string query = @$"SELECT subcategoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Subcategory ORDER BY name ASC;";

            var collection = await Connection.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }
    }
}
