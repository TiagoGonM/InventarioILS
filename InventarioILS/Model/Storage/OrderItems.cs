using Dapper;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    internal class OrderItems : SingletonStorage<OrderItem, OrderItems>
    {
        readonly string insertQuery = @"INSERT INTO OrderDetail (orderId, itemId, shipmentStateId, quantity)
                               VALUES (@OrderId, @ItemId, @ShipmentStateId, @Quantity)";

        public void Add(OrderItem item, IDbTransaction transaction = null)
        {
            using var conn = transaction?.Connection ?? CreateConnection();

            conn.Execute(insertQuery, new
            {
                item.OrderId,
                item.ItemId,
                item.ShipmentStateId,
                item.Quantity
            }, transaction: transaction);
        }

        public async Task AddAsync(OrderItem item, IDbTransaction transaction)
        {
            var conn = transaction.Connection;

            await conn.ExecuteAsync(insertQuery, new
            {
                item.OrderId,
                item.ItemId,
                item.ShipmentStateId,
                item.Quantity
            }, transaction: transaction).ConfigureAwait(false);
        }

        public void Load()
        {
            using var conn = CreateConnection();

            string query = @"SELECT ordDet.orderDetailId id, ord.name, it.productCode, it.description, c.name class, ss.name shipmentState, ordDet.quantity
                             FROM OrderDetail ordDet
                             JOIN 'Order' ord ON ordDet.orderId = ord.orderId
                             JOIN Item it ON ordDet.itemId = it.itemId
                             JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId
                             JOIN Class c ON it.classId = c.classId";

            var collection = conn.Query<OrderItem>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = CreateConnection();

            string query = @"SELECT ordDet.orderDetailId id, o.name, it.productCode, it.description, it.class, ss.name as shipmentState, ordDet.quantity
                             FROM OrderDetail ordDet
                             JOIN 'Order' o ON ordDet.orderId = o.orderId
                             JOIN Item it ON ordDet.itemId = it.itemId
                             JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId";

            var collection = await conn.QueryAsync<OrderItem>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        string selectSingleQuery = @"SELECT * FROM View_OrderItemsSummary WHERE orderId = @OrderId";

        public void LoadSingle(uint orderId)
        {
            using var conn = CreateConnection();

            var collection = conn.Query<OrderItem>(selectSingleQuery, new { OrderId = orderId });
            Items = collection.ToList().ToObservableCollection();
        }

        public async Task LoadSingleAsync(uint orderId)
        {
            using var conn = CreateConnection();

            var collection = await conn.QueryAsync<OrderItem>(selectSingleQuery, new { OrderId = orderId }).ConfigureAwait(false);
            Items = collection.ToList().ToObservableCollection();
        }

        public async Task UpdateAsync(OrderItem item)
        {
            // TODO
        }
    }
}
