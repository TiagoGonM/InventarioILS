using Dapper;
using System;
using System.Linq;


namespace InventarioILS.Model.Storage
{
    internal class OrderItems : SingletonStorage<OrderItem, OrderItems>, ILoadSave
    {
        // TODO: implement this
        public void Add(Item item)
        {
            throw new NotImplementedException();
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

        public async void Save()
        {
            throw new NotImplementedException();
        }
    }
}
