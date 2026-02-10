
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    public class ItemClasses : SingletonStorage<ItemMisc, ItemClasses>, ILoadSave<ItemMisc>
    {
        public ItemClasses()
        {
            Load();
        }

        public void Add(ItemMisc item)
        {
            throw new NotImplementedException();
        }

        public async Task<uint> AddAsync(ItemMisc item)
        {
            using var conn = await CreateConnectionAsync();

            string query = @"INSERT INTO Class (name) VALUES (@Name) ON CONFLICT DO NOTHING";
            await conn.ExecuteAsync(query, new { item.Name }).ConfigureAwait(false);

            uint rowid = await conn.ExecuteScalarAsync<uint>("SELECT classId FROM Class WHERE name = @Name COLLATE NOCASE", new
            {
                item.Name
            }).ConfigureAwait(false);

            await LoadAsync();
            return rowid;
        }

        public async Task<uint> AddAsync(ItemMisc item, IDbTransaction transaction)
        {
            var conn = transaction.Connection;

            string query = @"INSERT INTO Class (name) VALUES (@Name) ON CONFLICT DO NOTHING";
            await conn.ExecuteAsync(query, new { item.Name }, transaction).ConfigureAwait(false);

            uint rowid = await conn.ExecuteScalarAsync<uint>("SELECT classId FROM Class WHERE name = @Name COLLATE NOCASE", new
            {
                item.Name
            }).ConfigureAwait(false);

            return rowid;
        }

        public void Load()
        {
            using var conn = CreateConnection();

            string query = @$"SELECT 
                             classId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM Class ORDER BY name ASC";
            var collection = conn.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = CreateConnection();

            string query = @$"SELECT 
                             classId id, 
                             {SQLUtils.StringCapitalize()} name
                             FROM Class ORDER BY name ASC";
            var collection = await conn.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task<DeleteResult> DeleteAsync(uint classId)
        {
            using var conn = await CreateConnectionAsync();

            try
            {
                await conn.ExecuteAsync("DELETE FROM Class WHERE classId = @Id", new { Id = classId }).ConfigureAwait(false);
                await LoadAsync();

                return DeleteResult.Ok("Tipo de producto eliminado.");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return DeleteResult.Locked("Existen productos vinculados a este tipo de producto.");
            }
        }
    }
}
