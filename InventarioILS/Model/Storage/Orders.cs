using Dapper;
using System.Data;
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
            using var conn = CreateConnection();

            string query = @"INSERT INTO 'Order' (name, description) VALUES (@Name, @Description);
                             SELECT last_insert_rowid();";

            int rowId = conn.ExecuteScalar<int>(query, new
            {
                order.Name,
                order.Description
            });

            return rowId;
        }

        public async Task<int> AddAsync(Order order, IDbTransaction transaction = null)
        {
            string query = @"INSERT INTO 'Order' (name, description) VALUES (@Name, @Description);
                             SELECT last_insert_rowid();";

            using var conn = transaction?.Connection ?? CreateConnection();

            var count = await conn.QueryFirstAsync<int>(@"SELECT COUNT(*) total FROM 'Order'").ConfigureAwait(false);
            order.Name = $"Pedido #{count + 1}";

            int rowId = await conn.ExecuteScalarAsync<int>(query, new
            {
                order.Name,
                order.Description
            }, transaction: transaction);

            return rowId;
        }

        public void Load()
        {
            using var conn = CreateConnection();

            string query = @"SELECT o.orderId id, o.name, o.description, o.createdAt FROM 'Order' o";
            var collection = conn.Query<Order>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = CreateConnection();

            string query = @"SELECT o.orderId id, o.name, o.description, o.createdAt FROM 'Order' o";
            var collection = await conn.QueryAsync<Order>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }
    }
}
