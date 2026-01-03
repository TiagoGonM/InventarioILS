using Dapper;
using InventarioILS.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Model.Storage
{
    internal class StockItems : SingletonStorage<StockItem, StockItems>
    {
        public enum Filters
        {
            PRODUCT_CODE,
            KEYWORD,
            CLASS_NAME
        }

        public FiltersImpl<Filters> QueryFilters = new();

        public void Load()
        {
            using var conn = CreateConnection();

            string query = @$"SELECT * FROM View_ItemStockSummary WHERE 1=1";

            var parameters = new DynamicParameters();

            if (QueryFilters.FilterList.TryGetValue(Filters.PRODUCT_CODE, out string productCode))
            {
                query += " AND it.productCode LIKE @productCode COLLATE NOCASE";
                parameters.Add("productCode", $"%{productCode}%");
            }

            if (QueryFilters.FilterList.TryGetValue(Filters.KEYWORD, out string keyword))
            {
                query += " AND it.description LIKE @keyword COLLATE NOCASE";
                parameters.Add("keyword", $"%{keyword}%");
            }

            if (QueryFilters.FilterList.TryGetValue(Filters.CLASS_NAME, out string className))
            {
                query += " AND class.name LIKE @className COLLATE NOCASE";
                parameters.Add("className", $"%{className}%");
            }

            var collection = conn.Query<StockItem>(query, parameters);

            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            using var conn = CreateConnection();

            string query = @$"SELECT * FROM View_ItemStockSummary WHERE 1=1";

            var parameters = new DynamicParameters();

            if (QueryFilters.FilterList.TryGetValue(Filters.PRODUCT_CODE, out string productCode))
            {
                query += " AND productCode LIKE @productCode COLLATE NOCASE";
                parameters.Add("productCode", $"%{productCode}%");
            }

            if (QueryFilters.FilterList.TryGetValue(Filters.KEYWORD, out string keyword))
            {
                query += " AND description LIKE @keyword COLLATE NOCASE";
                parameters.Add("keyword", $"%{keyword}%");
            }

            if (QueryFilters.FilterList.TryGetValue(Filters.CLASS_NAME, out string className))
            {
                query += " AND class LIKE @className COLLATE NOCASE";
                parameters.Add("className", $"%{className}%");
            }

            var collection = await conn.QueryAsync<StockItem>(query, parameters).ConfigureAwait(false);

            await UpdateItemsAsync(collection.ToList().ToObservableCollection()).ConfigureAwait(false);
        }

        public async Task LoadNoStockAsync()
        {
            using var conn = CreateConnection();

            var collection = await conn.QueryAsync<StockItem>(@"SELECT * FROM View_NoStockItems").ConfigureAwait(false);
            await UpdateItemsAsync(collection.ToList().ToObservableCollection()).ConfigureAwait(false);
        }

        public async Task<(uint, uint)> AddAsync(Item item)
        {
            using var conn = CreateConnection();

            uint itemRowId = await ItemService.AddItemAsync(item, null);
            uint stockItemRowId = 0;

            if (item is StockItem stockItem)
            {
                for (int i = 0; i < stockItem.Quantity; i++)
                {
                    stockItemRowId = await conn.ExecuteScalarAsync<uint>(
                        SQLUtils.IncludeLastRowIdInserted(
                            @"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                              VALUES (@ItemId, @StateId, @Location, @AdditionalNotes)"
                        ),
                        new
                        {
                            ItemId = itemRowId,
                            stockItem.StateId,
                            stockItem.Location,
                            stockItem.AdditionalNotes
                        }
                    ).ConfigureAwait(false);
                }
            }

            Load();

            return (itemRowId, stockItemRowId);
        }

        public async Task AddRangeAsync(ObservableCollection<StockItem> items)
        {
            await Task.Run(async () =>
            {
                using var transaction = CreateConnection().BeginTransaction();
                using var conn = transaction.Connection ?? throw new InvalidOperationException("La conexión de la transacción es nula.");
                try
                {
                    foreach (var item in items)
                    {
                        for (int i = 0; i < item.Quantity; i++)
                        {
                            // A. Insertar en Item y obtener el ID.
                            uint? itemId = await ItemService.AddItemAsync(item, transaction).ConfigureAwait(false);
                            // B. Insertar en ItemStock usando el ID generado.
                            await conn.ExecuteAsync(
                                @"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                              VALUES (@ItemId, @StateId, @Location, @AdditionalNotes)",
                                new
                                {
                                    ItemId = itemId,
                                    item.StateId,
                                    item.Location,
                                    item.AdditionalNotes
                                },
                                transaction
                            ).ConfigureAwait(false);
                        }
                    }

                    // 2. Si todo fue bien, confirmar una vez.
                    transaction.Commit();
                    await LoadAsync();
                }
                catch
                {
                    // 3. Si hay un fallo, revertir todo el lote.
                    transaction.Rollback();
                    throw;
                }
            });
        }

        public async Task UpdateAsync(StockItem itemToUpdate, StockItem item)
        {
            using var conn = CreateConnection();

            var oldQuantity = Items.First(it => string.Equals(it.ProductCode, item.ProductCode)).Quantity;

            string query = @"UPDATE ItemStock
                             SET
                                location = @Location,
                                stateId = @StateId,
                                additionalNotes = @AdditionalNotes,
                                updatedAt = CURRENT_TIMESTAMP
                             WHERE location = @OldLocation
                             AND stateId = @OldStateId
                             AND itemId IN (SELECT itemId FROM Item WHERE productCode = @ProductCode)";

            await conn.ExecuteAsync(query, new 
            { 
                item.Location, 
                item.StateId, 
                item.AdditionalNotes,
                OldLocation = itemToUpdate.Location,
                OldStateId = itemToUpdate.StateId,
                item.ProductCode
            }).ConfigureAwait(false);

            if (item.Quantity < oldQuantity)
            {
                await DeleteAsync(item, oldQuantity - item.Quantity);
            }

            await LoadAsync();
        }

        public async Task<int> DeleteAsync(StockItem item, uint quantityToDelete)
        {
            using var transaction = CreateConnection().BeginTransaction();
            using var conn = transaction.Connection ?? throw new InvalidOperationException("La conexión de la transacción es nula.");

            try
            {
                // Seleccionamos los IDs que coinciden con el código Y la ubicación
                var idsToDelete = (await conn.QueryAsync<int>(@"SELECT i.itemId 
                                                              FROM Item i
                                                              JOIN ItemStock s ON i.itemId = s.itemId
                                                              WHERE i.productCode = @ProductCode 
                                                              AND s.location = @Location
                                                              AND S.stateId = @StateId
                                                              AND i.isDeleted = 0
                                                              LIMIT @Quantity",
                new { item.ProductCode, item.Location, item.StateId, Quantity = quantityToDelete }, transaction)).ToList();

                if (idsToDelete.Count == 0) return 0;

                // Procedemos con el borrado lógico (isDeleted = 1) para no romper OrderDetail
                await conn.ExecuteAsync("UPDATE Item SET isDeleted = 1 WHERE itemId IN @Ids", new { Ids = idsToDelete }, transaction);

                transaction.Commit();
                return idsToDelete.Count;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
