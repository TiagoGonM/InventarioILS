using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    public class ItemCategories : SingletonStorage<ItemMisc, ItemCategories>
    {
        public ItemCategories()
        {
            Load();
        }

        public async Task AddAsync(ItemMisc item, IDbTransaction transaction)
        {
            var conn = transaction.Connection;

            string query = @"INSERT INTO Category (name, shorthand) VALUES (@Name, @Shorthand)";

            var collection = await conn.ExecuteAsync(query, new
            {
                item.Name,
                item.Shorthand,
            }, transaction).ConfigureAwait(false);
        }

        public async Task LinkWithAsync(uint subcategoryId, uint categoryId, IDbTransaction transaction)
        {
            var conn = transaction.Connection;

            string query = @"INSERT INTO CatSubcat (categoryId, subcategoryId) VALUES (@CatId, @SubcatId)";

            await conn.ExecuteAsync(query, new {CatId = categoryId, SubcatId = subcategoryId}, transaction).ConfigureAwait(false);
        }

        public void Load()
        {
            using var conn = CreateConnection();

            string query = @$"SELECT categoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Category ORDER BY name ASC";
            var collection = conn.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = await CreateConnectionAsync();

            string query = @$"SELECT categoryId id, {SQLUtils.StringCapitalize()} name, shorthand FROM Category ORDER BY name ASC";
            var collection = await conn.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task<DeleteResult> DeleteAsync(uint categoryId)
        {
            using var conn = await CreateConnectionAsync();

            try
            {
                await conn.ExecuteAsync("DELETE FROM Category WHERE categoryId = @Id", new { Id = categoryId }).ConfigureAwait(false);
                await LoadAsync();

                return DeleteResult.Ok("Categoría eliminada.");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return DeleteResult.Locked("Existen productos vinculados a esta categoría.");
            }
        }
    }
}
