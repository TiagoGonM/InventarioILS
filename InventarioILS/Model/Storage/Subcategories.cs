using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    public class ItemSubCategories : SingletonStorage<ItemMisc, ItemSubCategories>
    {
        uint filterById = 0;

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

        public void Load(uint categoryId = 0)
        {
            var id = categoryId | filterById;

            using var conn = CreateConnection();

            string query = @$"SELECT sub.subcategoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Subcategory sub";
            if (id > 0) query += @" JOIN CatSubcat cs ON sub.subcategoryId = cs.subcategoryId
                                            WHERE cs.categoryId = @CategoryId";
            query += " ORDER BY name ASC";

            var collection = conn.Query<ItemMisc>(query, new { CategoryId = id });
            UpdateItems(collection.ToList().ToObservableCollection());

            if (id > 0) filterById = id;
        }

        public async Task LoadAsync(uint categoryId = 0)
        {
            var id = categoryId > 0 ? categoryId : filterById;

            using var conn = await CreateConnectionAsync();

            string query = @$"SELECT cs.subcategoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Subcategory sub
                              JOIN CatSubcat cs ON sub.subcategoryId = cs.subcategoryId";
            if (id > 0) query += " WHERE cs.categoryId = @CategoryId";
            query += " ORDER BY name ASC";

            var collection = await conn.QueryAsync<ItemMisc>(query, new { CategoryId = id }).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());

            if (id > 0) filterById = id;
        }

        public async Task<DeleteResult> DeleteAsync(uint subcategoryId)
        {
            using var conn = await CreateConnectionAsync();

            try
            {
                await conn.ExecuteAsync("DELETE FROM Subcategory WHERE subcategoryId = @Id", new { Id = subcategoryId }).ConfigureAwait(false);
                await LoadAsync();

                return DeleteResult.Ok("Subcategoría eliminada.");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return DeleteResult.Locked("Existen productos vinculados a esta subcategoría.");
            }
        }
    }
}
