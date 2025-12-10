using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    internal class Orders : SingletonStorage<Order, Orders>
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

        public int Add(Order order)
        {
            string query = @"INSERT INTO 'Order' (name, description) VALUES (@Name, @Description);
                             SELECT last_insert_rowid();";

            int rowId = Connection.ExecuteScalar<int>(query, new
            {
                order.Name,
                order.Description
            });

            return rowId;
        }

        public async Task<int> AddAsync(Order order)
        {
            string query = @"INSERT INTO 'Order' (name, description) VALUES (@Name, @Description);
                             SELECT last_insert_rowid();";

            int rowId = await Connection.ExecuteScalarAsync<int>(query, new
            {
                order.Name,
                order.Description
            });

            return rowId;
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
