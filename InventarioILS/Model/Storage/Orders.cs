using Dapper;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    public class Orders : SingletonStorage<Order, Orders>
    {
        public enum Filters
        {
            DESCRIPTION
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

        public async Task<int> AddAsync(Order order, IDbTransaction transaction)
        {
            var conn = transaction.Connection;
            
            string query = @"INSERT INTO 'Order' (name, description) VALUES (@Name, @Description);
                             SELECT last_insert_rowid();";


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

            string query = @"SELECT o.orderId id, o.name, o.description, o.done, o.createdAt FROM 'Order' o WHERE 1=1";

            var parameters = new DynamicParameters();

            if (QueryFilters.FilterList.TryGetValue(Filters.DESCRIPTION, out string description))
            {
                query += @" AND o.description LIKE @description COLLATE NOCASE";
                parameters.Add("description", $"%{description}%");
            }

            var collection = conn.Query<Order>(query, parameters);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = await CreateConnectionAsync();

            string query = @"SELECT o.orderId id, o.name, o.description, o.done, o.createdAt FROM 'Order' o WHERE 1=1";

            var parameters = new DynamicParameters();

            if (QueryFilters.FilterList.TryGetValue(Filters.DESCRIPTION, out string description))
            {
                query += @" AND o.description LIKE @description COLLATE NOCASE";
                parameters.Add("description", $"%{description}%");
            }

            var collection = await conn.QueryAsync<Order>(query, parameters).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadDoneAsync()
        {
            using var conn = await CreateConnectionAsync();

            string query = @"SELECT * FROM View_ShowCompletedOrders";

            var collection = await conn.QueryAsync<Order>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task MarkAsDoneAsync(uint orderId)
        {
            using var conn = await CreateConnectionAsync();

            string query = @"UPDATE `Order` 
                             SET
                                done = @Done
                             WHERE orderId = @OrderId";

            await conn.ExecuteAsync(query, new
            {
                Done = 1,
                OrderId = orderId
            });

            await LoadAsync();
        }
    }
}
