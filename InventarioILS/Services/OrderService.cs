using InventarioILS.Model;
using InventarioILS.Model.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Services
{
    public class OrderService
    {
        readonly static Orders orderStorage = Orders.Instance;
        readonly static ShipmentStates stateStorage = ShipmentStates.Instance;
        readonly static OrderItems orderItemStorage = OrderItems.Instance;
        
        public static async Task RegisterOrder(Order order, IEnumerable<OrderItem> items)
        {
            // 1. Añadir un Order nuevo
            // 2. Añadir tantas filas en Item como la cantidad indicada
            // 3. Añadir tantas filas en OrderDetail como la cantidad indicada con cantidad = 1 (campo redundante), shipmentState = pendiente y el id del item insertado previamente al crearlo

            if (order == null || items == null) return;

            using var client = await DbConnection.CreateAndOpenAsync();
            using var transaction = client.BeginTransaction();

            try
            {
                int orderId = await orderStorage.AddAsync(order, transaction);

                await stateStorage.LoadAsync();

                var defaultStateId = stateStorage.GetStateId("pendiente");

                foreach (var orderItem in items)
                {
                    orderItem.ShipmentStateId ??= defaultStateId;

                    uint itemsToAdd = orderItem.Quantity;

                    for (int i = 0; i < itemsToAdd; i++)
                    {
                        uint itemId = await ItemService.AddItemAsync(orderItem, transaction);

                        if (itemId == 0) throw new ArgumentNullException($"Item id no insertado: {itemId}");

                        await orderItemStorage.AddAsync(new OrderItem
                        {
                            OrderId = (uint)orderId,
                            ItemId = itemId,
                            ShipmentStateId = orderItem.ShipmentStateId,
                            Quantity = 1,
                        }, transaction);
                    }
                }

                transaction.Commit();
                //orderStorage.Load();
            }
            catch
            {
                if (transaction.Connection != null)
                {
                    transaction.Rollback();
                }
                throw;
            }
        }
    }
}
