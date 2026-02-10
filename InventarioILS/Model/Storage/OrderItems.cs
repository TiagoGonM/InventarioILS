using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventarioILS.Model.Storage
{
    public class OrderItems : SingletonStorage<OrderItem, OrderItems>
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

            string query = @"INSERT INTO OrderDetail (orderId, itemId, shipmentStateId, quantity)
                             VALUES (@OrderId, @ItemId, @ShipmentStateId, @Quantity)";

            try
            {
                await conn.ExecuteAsync(query, new
                {
                    item.OrderId,
                    item.ItemId,
                    item.ShipmentStateId,
                    item.Quantity
                }, transaction);
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                throw new ApplicationException($"Código: {ex.SqliteErrorCode}, Extendido: {ex.SqliteExtendedErrorCode}", ex);
            }
        }

        public void Load()
        {
            using var conn = CreateConnection();

            string query = @"SELECT * from View_OrderItemsSummary";

            var collection = conn.Query<OrderItem>(query);
            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = await CreateConnectionAsync();

            string query = @"SELECT * from View_OrderItemsSummary";

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
            using var conn = await CreateConnectionAsync();

            var collection = await conn.QueryAsync<OrderItem>(selectSingleQuery, new { OrderId = orderId }).ConfigureAwait(false);
            Items = collection.ToList().ToObservableCollection();
        }

        string updateQuery = @"UPDATE OrderDetail
                                SET shipmentStateId = @ShipmentStateId
                            FROM Item
                            WHERE OrderDetail.itemId = Item.itemId
                              AND OrderDetail.orderId = @OrderId
                              AND Item.productCode = @ProductCode COLLATE NOCASE";

        public async Task UpdateAsync(string productCode, uint newShipmentStateId, uint orderId)
        {
            using var conn = await CreateConnectionAsync();

            try
            {
                await conn.ExecuteAsync(updateQuery, new
                {
                    ShipmentStateId = newShipmentStateId,
                    OrderId = orderId,
                    ProductCode = productCode
                });
            } catch (SqliteException ex)
            {
                await StatusManager.Instance.UpdateMessageStatusAsync($"Error al tratar de actualizar el estado del envio: {ex}", StatusManager.MessageType.ERROR);
                throw;
            }

            await LoadSingleAsync(orderId);
        }

        public async Task MarkAsReceived(IEnumerable<OrderItem> items)
        {

            var orderId = items.First().OrderId;
            await Task.Run(async () =>
            {
                using var initialConn = await CreateConnectionAsync();
                using var transaction = await initialConn.BeginTransactionAsync();
                var conn = transaction.Connection;

                string query = @"UPDATE OrderDetail
                                    SET received = @Received
                                 FROM Item
                                 WHERE OrderDetail.itemId = Item.itemId
                                    AND OrderDetail.orderId = @OrderId
                                    AND Item.productCode = @ProductCode COLLATE NOCASE";
                
                foreach (var item in items)
                {
                    try
                    {
                        for (var i = 0; i < item.Quantity; i++)
                        {
                            await conn.ExecuteAsync(query, new
                            {
                                Received = true,
                                OrderId = (uint)orderId,
                                item.ProductCode
                            }, transaction).ConfigureAwait(false);
                        }
                    }
                    catch (SqliteException ex)
                    {
                        await StatusManager.Instance.UpdateMessageStatusAsync(
                            $"Error al tratar de actualizar el estado del envío del item {item.ProductCode}: {ex}", StatusManager.MessageType.ERROR);
                        
                        transaction.Rollback();
                        throw;
                    }
                    
                }
                transaction.Commit();
                await LoadSingleAsync((uint)orderId);
            });

        }
    }
}
