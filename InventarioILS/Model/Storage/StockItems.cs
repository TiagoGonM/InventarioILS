using Dapper;
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

        public void Save()
        {
        }

        public void Load()
        {
            if (Connection == null) return;

            string query = @$"SELECT sto.itemStockId id, it.productCode, c.name category, s.name subcategory, {SQLUtils.StringCapitalize("class.name")} class, it.description, {SQLUtils.StringCapitalize("st.name")} state, {SQLUtils.StringCapitalize("sto.location")} location, sto.additionalNotes, COUNT(*) quantity
                                 FROM ItemStock sto
                                 JOIN Item it ON sto.itemId = it.itemId
                                 JOIN Class class ON it.classId = class.classId
                                 JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                                 JOIN Category c ON cs.categoryId = c.categoryId
                                 JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                                 JOIN State st ON sto.stateId = st.stateId
                                 WHERE 1=1";

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
                        LIMIT 50;";

            var collection = Connection.Query<StockItem>(query, parameters);

            UpdateItems(collection.ToList().ToObservableCollection());
        }

        public async Task LoadAsync()
        {
            if (Connection == null) return;

            string query = @$"SELECT sto.itemStockId id, it.productCode, c.name category, s.name subcategory, {SQLUtils.StringCapitalize("class.name")} class, it.description, {SQLUtils.StringCapitalize("st.name")} state, {SQLUtils.StringCapitalize("sto.location")} location, sto.additionalNotes, COUNT(*) quantity
                                 FROM ItemStock sto
                                 JOIN Item it ON sto.itemId = it.itemId
                                 JOIN Class class ON it.classId = class.classId
                                 JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
                                 JOIN Category c ON cs.categoryId = c.categoryId
                                 JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
                                 JOIN State st ON sto.stateId = st.stateId
                                 WHERE 1=1";

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
                        LIMIT 50;";

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
                            int itemId = await Utils.AddItemAsync(item, transaction).ConfigureAwait(false);
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
    }
}
