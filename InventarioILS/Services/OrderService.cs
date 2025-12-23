using Dapper;
using InventarioILS.Model;
using InventarioILS.Model.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventarioILS.Services
{
    internal class OrderService
    {
        readonly static Orders orderStorage = Orders.Instance;
        readonly static ShipmentStates stateStorage = ShipmentStates.Instance;
        readonly static OrderItems orderItemStorage = OrderItems.Instance;
        
        public static async Task RegisterOrder(Order order, IEnumerable<OrderItem> items)
        {
            // 1. Añadir un Order nuevo
            // 2. Añadir tantas filas en Item como la cantidad indicada
            // 3. Añadir tantas filas en OrderDetail como la cantidad indicada con cantidad = 1 (campo redundante), shipmentState = pendiente y el id del item insertado previamente al crearlo

            await Task.Run(async () =>
            {
                using var transaction = new DbConnection().BeginTransaction();
                using var conn = transaction.Connection ?? throw new InvalidOperationException("La conexión de la transacción es nula.");
                    
                try
                {
                    int orderId = await orderStorage.AddAsync(order, transaction);

                    await stateStorage.LoadAsync();
                    var defaultStateId = stateStorage.Items.FirstOrDefault(
                        state => state.Name.Equals("pendiente", StringComparison.OrdinalIgnoreCase))?.Id;

                    foreach (var orderItem in items)
                    {
                        orderItem.ShipmentStateId ??= defaultStateId;

                        uint itemsToAdd = orderItem.Quantity;

                        for (int i = 0; i < itemsToAdd; i++)
                        {
                            uint? itemId = await ItemService.AddItemAsync(orderItem, transaction);

                            if (!itemId.HasValue) throw new ArgumentNullException($"Item id not inserted: {itemId}");

                            orderItem.OrderId = (uint)orderId;
                            orderItem.ItemId = itemId;
                            orderItem.Quantity = 1; // Cada fila en OrderDetail representa una unidad individual

                            await orderItemStorage.AddAsync(orderItem, transaction);
                        }
                    }
                } catch
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            });
            orderStorage.Load();
        }
    }
}
