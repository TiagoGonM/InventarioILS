using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    internal class Orders : SingletonStorage<Order, Orders>, ILoadSave
    {
        public enum Filters
        {
            PRODUCT_CODE,
            KEYWORD,
            CLASS_NAME
        }

        public FiltersImpl<Filters> QueryFilters = new();

        public Orders()
        {
            Load();
        }

        public void Add(Item item)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            string query = @"SELECT o.orderId id, o.name, o.description, o.createdAt FROM 'Order' o;";
            var collection = Connection.Query<Order>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async void LoadAsync()
        {
            string query = @"SELECT o.orderId id, o.name, o.description, o.createdAt FROM 'Order' o;";
            var collection = await Connection.QueryAsync<Order>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
