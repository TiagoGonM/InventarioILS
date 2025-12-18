using Dapper;
using System;
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
            if (Connection == null) return;

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

            query += @" GROUP BY 
                            it.productCode,
                            c.name,
                            s.name,
                            class.name
                        LIMIT 50";

            var collection = Connection.Query<StockItem>(query, parameters);

            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            if (Connection == null) return;

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

            var collection = await Connection.QueryAsync<StockItem>(query, parameters).ConfigureAwait(false);

            await UpdateItemsAsync(collection.ToList().ToObservableCollection()).ConfigureAwait(false);
        }

        public (int, int) Add(Item item)
        {
            int itemRowId = Utils.AddItem(item);
            int stockItemRowId = -1;

            if (item is StockItem stockItem)
            {
                for (int i = 0; i < stockItem.Quantity; i++)
                {
                    stockItemRowId = Connection.ExecuteScalar<int>(
                        @"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                            VALUES (@ItemId, @StateId, @Location, @AdditionalNotes);
                            SELECT last_insert_rowid();",
                        new
                        {
                            ItemId = itemRowId,
                            stockItem.StateId,
                            stockItem.Location,
                            stockItem.AdditionalNotes
                        }
                    );
                }
            }

            Load();

            return (itemRowId, stockItemRowId);
        }

        public async Task<(int, int)> AddAsync(Item item)
        {
            int itemRowId = Utils.AddItem(item);
            int stockItemRowId = -1;

            if (item is StockItem stockItem)
            {
                for (int i = 0; i < stockItem.Quantity; i++)
                {
                    stockItemRowId = await Connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                          VALUES (@ItemId, @StateId, @Location, @AdditionalNotes);
                          SELECT last_insert_rowid();",
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


        public void AddRange(ObservableCollection<StockItem> items)
        {
            if (items.Count == 0) return;

            foreach (var item in items)
            {
                for (int i = 0; i < item.Quantity; i++)
                {
                    Add(item);
                }
            }
        }

        public async Task AddRangeAsync(ObservableCollection<StockItem> items)
        {
            await Task.Run(async () =>
            {
                using var transaction = Connection.BeginTransaction();
                try
                {
                    foreach (var item in items)
                    {
                        for (int i = 0; i < item.Quantity; i++)
                        {
                            // A. Insertar en Item y obtener el ID.
                            uint? itemId = await Utils.AddItemAsync(item, transaction).ConfigureAwait(false);
                            // B. Insertar en ItemStock usando el ID generado.
                            await Connection.ExecuteAsync(
                                @"INSERT INTO ItemStock (itemId, stateId, location, additionalNotes)
                              VALUES (@ItemId, @StateId, @Location, @AdditionalNotes)",
                                new
                                {
                                    ItemId = itemId,
                                    item.StateId,
                                    item.Location,
                                    item.AdditionalNotes
                                },
                                transaction: transaction
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

        public async Task UpdateAsync(StockItem item)
        {
            var oldQuantity = Items.First(it => string.Equals(it.ProductCode, item.ProductCode)).Quantity;

            string query = @"UPDATE ItemStock
                             SET
                                location = @Location,
                                stateId = @StateId,
                                additionalNotes = @AdditionalNotes,
                                updatedAt = CURRENT_TIMESTAMP
                             WHERE itemId = (SELECT itemId FROM Item WHERE productCode = @ProductCode)";

            await Connection.ExecuteAsync(query, new 
            { 
                item.Location, 
                item.StateId, 
                item.AdditionalNotes, 
                item.ProductCode
            }).ConfigureAwait(false);

            if (item.Quantity < oldQuantity)
            {
                await DeleteAsync(item.ProductCode, oldQuantity - item.Quantity);
            }

            await LoadAsync();
        }

        // TODO: implement bulk deleting
        public async Task DeleteAsync(string productCode, uint quantityToDelete)
        {
            //string sql = @"DELETE FROM ItemStock 
            //               WHERE itemStockId IN (
            //                   SELECT itemStockId 
            //                   FROM ItemStock 
            //                   WHERE itemId = (SELECT itemId FROM Item WHERE productCode = @ProductCode)
            //                   ORDER BY createdAt ASC
            //                   LIMIT @Quantity
            //               );";

            //await Connection.ExecuteAsync(sql, new
            //{ 
            //    ProductCode = productCode, 
            //    Quantity = quantityToDelete 
            //}).ConfigureAwait(false);
        }
    }
}
