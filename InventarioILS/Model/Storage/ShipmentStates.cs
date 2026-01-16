using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    internal class ShipmentStates : SingletonStorage<ItemMisc, ShipmentStates>
    {
        public void Load()
        {
            using var conn = CreateConnection();

            string query = @$"SELECT shipmentStateId id, {SQLUtils.StringCapitalize()} name FROM ShipmentState";

            var collection = conn.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = await CreateConnectionAsync();

            string query = @$"SELECT shipmentStateId id, {SQLUtils.StringCapitalize()} name FROM ShipmentState";

            var collection = await conn.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public uint GetStateId(string name)
        {
            var defaultStateId = Items.FirstOrDefault(
                        state => state.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Id;

            if (!defaultStateId.HasValue) return 0;
            
            return (uint)defaultStateId;
        }

        public void Add(string name)
        {
            using var conn = CreateConnection();

            conn.Execute(@"INSERT INTO ShipmentState (name) VALUES (@Name)");

            Load();
        }

        public async Task AddAsync(string name)
        {
            using var conn = await CreateConnectionAsync();

            await conn.ExecuteAsync(@"INSERT INTO ShipmentState (name) VALUES (@Name)", new { Name = name }).ConfigureAwait(false);

            await LoadAsync();
        }

        public async Task<DeleteResult> DeleteAsync(uint shipmentStateId)
        {
            using var conn = await CreateConnectionAsync();

            try
            {
                await conn.ExecuteAsync("DELETE FROM ShipmentState WHERE shipmentStateId = @Id", new { Id = shipmentStateId }).ConfigureAwait(false);

                await LoadAsync();

                return DeleteResult.Ok("Estado de envío eliminado.");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return DeleteResult.Locked("Existen productos vinculados a este estado de envío.");
            }
        }
    }
}
