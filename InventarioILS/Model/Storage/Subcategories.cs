using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    public class ItemSubcategories : SingletonStorage<ItemMisc, ItemSubcategories>
    {
        uint filterById = 0;

        public ItemSubcategories()
        {
            Load();
        }

        string addQuery = @"INSERT INTO Subcategory (name, shorthand) VALUES (@Name, @Shorthand) ON CONFLICT(name) DO NOTHING";
        public uint Add(ItemMisc item)
        {
            using var conn = CreateConnection();

            if (string.IsNullOrWhiteSpace(item.Shorthand)) item.Shorthand = null;

            conn.Execute(addQuery, new
            {
                Name = item.Name.ToLower(),
                item.Shorthand
            });

            uint rowid = conn.ExecuteScalar<uint>("SELECT subcategoryId FROM Subcategory WHERE name = @Name", new { item.Name });

            Load();
            return rowid;
        }

        public async Task<uint> AddAsync(ItemMisc item)
        {
            using var conn = await CreateConnectionAsync();

            if (string.IsNullOrEmpty(item.Shorthand)) item.Shorthand = null;
            
            await conn.ExecuteAsync(addQuery, new
            {
                Name = item.Name.ToLower(),
                item.Shorthand
            }).ConfigureAwait(false);

            uint rowid = await conn.ExecuteScalarAsync<uint>("SELECT subcategoryId FROM Subcategory WHERE name = @Name COLLATE NOCASE", new 
            {
                item.Name
            }).ConfigureAwait(false);

            await LoadAsync();
            return rowid;
        }

        public async Task<uint> AddAsync(ItemMisc item, IDbTransaction transaction)
        {
            var conn = transaction.Connection;

            if (string.IsNullOrEmpty(item.Shorthand)) item.Shorthand = null;

            await conn.ExecuteAsync(addQuery, new
            {
                Name = item.Name.ToLower(),
                item.Shorthand
            }, transaction).ConfigureAwait(false);

            uint rowid = await conn.ExecuteScalarAsync<uint>("SELECT subcategoryId FROM Subcategory WHERE name = @Name COLLATE NOCASE", new
            {
                item.Name
            }).ConfigureAwait(false);

            return rowid;
        }

        public async Task AddRangeAsync(ItemMisc[] items)
        {
            using var conn = await CreateConnectionAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                foreach (var item in items)
                {
                    if (string.IsNullOrWhiteSpace(item.Shorthand)) item.Shorthand = null;
                    await conn.ExecuteAsync(addQuery, new
                    {
                        Name = item.Name.ToLower(),
                        item.Shorthand
                    }, transaction).ConfigureAwait(false);
                }

                await transaction.CommitAsync().ConfigureAwait(false);
                await LoadAsync();
            } catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
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
