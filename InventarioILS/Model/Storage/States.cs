using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    public class ItemStates : SingletonStorage<ItemMisc, ItemStates>
    {
        uint filterById = 0;

        public ItemStates()
        {
            Load();
        }

        public void Add(ItemMisc item)
        {
            throw new NotImplementedException();
        }

        public async Task<uint> AddAsync(ItemMisc item, uint classId)
        {
            using var conn = await CreateConnectionAsync();

            string query = @"INSERT INTO State (name, classId) VALUES (@Name, @ClassId) ON CONFLICT DO NOTHING";
            await conn.ExecuteAsync(query, new { item.Name, ClassId = classId }).ConfigureAwait(false);

            uint rowid = await conn.ExecuteScalarAsync<uint>("SELECT stateId FROM State WHERE name = @Name COLLATE NOCASE", new
            {
                item.Name
            }).ConfigureAwait(false);

            await LoadAsync();
            return rowid;
        }

        public async Task<uint> AddAsync(ItemMisc item, uint classId, IDbTransaction transaction)
        {
            var conn = transaction.Connection;

            string query = @"INSERT INTO State (name, classId) VALUES (@Name, @ClassId) ON CONFLICT DO NOTHING";
            await conn.ExecuteAsync(query, new { item.Name, ClassId = classId }, transaction).ConfigureAwait(false);

            uint rowid = await conn.ExecuteScalarAsync<uint>("SELECT stateId FROM State WHERE name = @Name COLLATE NOCASE", new
            {
                item.Name
            }).ConfigureAwait(false);

            return rowid;
        }

        public void Load(uint classId = 0)
        {
            var id = classId | filterById;

            using var conn = CreateConnection();
            string query = @$"SELECT 
                             stateId id,
                             {SQLUtils.StringCapitalize()} name
                             FROM State
                             WHERE 1=1";

            if (id > 0) query += @$" AND classId = @ClassId";

            query += " ORDER BY name ASC";

            var collection = conn.Query<ItemMisc>(query, new { ClassId = id });
            UpdateItems(collection.ToList().ToObservableCollection());

            if (id > 0) filterById = id;
        }

        public async Task LoadAsync(uint classId = 0)
        {
            var id = classId | filterById;

            using var conn = await CreateConnectionAsync();
            string query = @$"SELECT 
                             stateId id,
                             {SQLUtils.StringCapitalize()} name
                             FROM State
                             WHERE 1=1";

            if (id > 0) query += @$" AND classId = @ClassId";

            query += " ORDER BY name ASC";
            
            var collection = await conn.QueryAsync<ItemMisc>(query, new { ClassId = id }).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());

            if (id > 0) filterById = id;
        }

        public async Task<DeleteResult> DeleteAsync(uint stateId)
        {
            using var conn = await CreateConnectionAsync();

            try
            {
                await conn.ExecuteAsync("DELETE FROM State WHERE stateId = @Id", new { Id = stateId }).ConfigureAwait(false);
                await LoadAsync();

                return DeleteResult.Ok("Estado eliminada.");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return DeleteResult.Locked("Existen productos vinculados a este estado.");
            }
        }
    }
}
