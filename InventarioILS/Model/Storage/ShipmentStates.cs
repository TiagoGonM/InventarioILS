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
            string query = @$"SELECT shipmentStateId id, {SQLUtils.StringCapitalize()} name FROM ShipmentState";

            var collection = Connection.Query<ItemMisc>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            string query = @$"SELECT shipmentStateId id, {SQLUtils.StringCapitalize()} name FROM ShipmentState";

            var collection = await Connection.QueryAsync<ItemMisc>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }
    }
}
