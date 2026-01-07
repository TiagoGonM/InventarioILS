using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    internal class ShipmentStates : SingletonStorage<ItemMisc, ShipmentStates>, ILoadSave<ItemMisc>
    {
        public void Add(ItemMisc item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            using var conn = CreateConnection();

            string query = @$"SELECT shipmentStateId id, {SQLUtils.StringCapitalize()} name FROM ShipmentState";

            var collection = conn.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = CreateConnection();

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
    }
}
