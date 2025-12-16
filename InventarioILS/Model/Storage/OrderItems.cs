using Dapper;
using System.Linq;


namespace InventarioILS.Model.Storage
{
    internal class OrderItems : SingletonStorage<OrderItem, OrderItems>, ILoadSave<Item>
    {
        // TODO: implement this
        public void Add(Item item)
        {
            string query = @"INSERT INTO OrderDetail (orderId, itemId, shipmentStateId, quantity)
                             VALUES (@OrderId, @ItemId, @ShipmentStateId, @Quantity);";

            // 1. Añadir un Order nuevo
            // 2. Añadir tantas filas en Item por como la cantidad indicada
            // 3. Añadir tantas filas en OrderDetail como la cantidad indicada con cantidad = 1 (campo redundante), shipmentState = pendiente y el id del item insertado previamente al crearlo

            //Connection.Execute(query, new
            //{
            //    OrderId = LastRowInserted,

            //};
        }

        public void Load()
        {
            if (Connection == null) return;

            string query = @"SELECT ordDet.orderDetailId id, ord.name, it.productCode, it.description, c.name class, ss.name shipmentState, ordDet.quantity
                             FROM OrderDetail ordDet
                             JOIN 'Order' ord ON ordDet.orderId = ord.orderId
                             JOIN Item it ON ordDet.itemId = it.itemId
                             JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId
                             JOIN Class c ON it.classId = c.classId;";

            var collection = Connection.Query<OrderItem>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async void LoadAsync()
        {
            if (Connection == null) return;

            string query = @"SELECT ordDet.orderDetailId id, o.name, it.productCode, it.description, it.class, ss.name as shipmentState, ordDet.quantity
                             FROM OrderDetail ordDet
                             JOIN 'Order' o ON ordDet.orderId = o.orderId
                             JOIN Item it ON ordDet.itemId = it.itemId
                             JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId;";

            var collection = await Connection.QueryAsync<OrderItem>(query).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public void LoadSingle(uint orderId)
        {
            if (Connection == null) return;

            string query = @"SELECT ordDet.orderDetailId id, ord.name, it.productCode, it.description, c.name class, ss.name shipmentState, ordDet.quantity
                             FROM OrderDetail ordDet
                             JOIN 'Order' ord ON ordDet.orderId = ord.orderId
                             JOIN Item it ON ordDet.itemId = it.itemId
                             JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId
                             JOIN Class c ON it.classId = c.classId 
                             WHERE ord.orderId = @OrderId;";

            var collection = Connection.Query<OrderItem>(query, new { OrderId = orderId });
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async void LoadSingleAsync(uint orderId)
        {
            if (Connection == null) return;

            string query = @"SELECT ordDet.orderDetailId id, ord.name, it.productCode, it.description, c.name class, ss.name shipmentState, ordDet.quantity
                             FROM OrderDetail ordDet
                             JOIN 'Order' ord ON ordDet.orderId = ord.orderId
                             JOIN Item it ON ordDet.itemId = it.itemId
                             JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId
                             JOIN Class c ON it.classId = c.classId 
                             WHERE ord.orderId = @OrderId;";

            var collection = await Connection.QueryAsync<OrderItem>(query, new { OrderId = orderId }).ConfigureAwait(false);
            UpdateItems(collection.ToList().ToObservableCollection());
        }
    }
}
