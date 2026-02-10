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

        public async Task<uint> AddAsync(ItemMisc item, IDbTransaction transaction)
        {
            var conn = transaction.Connection;

            string query = @"INSERT INTO Category (name, shorthand) VALUES (@Name, @Shorthand) ON CONFLICT DO NOTHING";

            await conn.ExecuteAsync(query, new
            {
                Name = item.Name.ToLower(),
                item.Shorthand,
            }, transaction).ConfigureAwait(false);

            uint rowid = await conn.ExecuteScalarAsync<uint>("SELECT categoryId FROM Category WHERE name = @Name COLLATE NOCASE", new
            {
                item.Name
            }).ConfigureAwait(false);

            return rowid;
        }

        public async Task LinkWithAsync(uint subcategoryId, uint categoryId, IDbTransaction transaction)
        {
            var conn = transaction.Connection;

            string query = @"INSERT INTO CatSubcat (categoryId, subcategoryId) VALUES (@CatId, @SubcatId) ON CONFLICT DO NOTHING";

            await conn.ExecuteAsync(query, new { CatId = categoryId, SubcatId = subcategoryId }, transaction);
        }

        public async Task<int> EnsureCatSubcatIdAsync(uint catId, uint subId, IDbConnection conn, IDbTransaction trans)
        {
            // 1. Intentamos insertar por si no existe
            await conn.ExecuteAsync(@"
                INSERT INTO CatSubcat (categoryId, subcategoryId) 
                VALUES (@catId, @subId) 
                ON CONFLICT DO NOTHING", new { catId, subId }, trans);

            // 2. Buscamos el ID (ahora SI O SI tiene que estar)
            var id = await conn.QuerySingleOrDefaultAsync<int?>(@"
                SELECT catSubcatId FROM CatSubcat 
                WHERE categoryId = @catId AND subcategoryId = @subId",
                new { catId, subId }, trans);

            return id == null
                ? throw new Exception($"¡Error fatal! No se pudo crear ni encontrar el vínculo Cat:{catId} Sub:{subId}")
                : id.Value;
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
