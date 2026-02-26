using CsvHelper;
using CsvHelper.Configuration;
using InventarioILS.Model;
using InventarioILS.Model.Serializables;
using InventarioILS.Model.Storage;
using InventarioILS.Model.Wizard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Services
{
    public class DataImportService
    {
        public class DataResponse {
            public List<StockItemDraft> Records { get; set; } = [];
            public List<ItemMisc> CategoryRecords { get; set; } = [];
            public List<ItemMisc> SubcategoryRecords { get; set; } = [];
            public List<ItemMisc> ClassRecords { get; set; } = [];
            public List<ItemMisc> StateRecords { get; set; } = [];

            public Map<ItemMisc, ItemMisc[]> LinkMap { get; set; } = [];

            public DataResponse(
                List<StockItemDraft> records,
                List<ItemMisc> categoryRecords,
                List<ItemMisc> subcategoryRecords,
                List<ItemMisc> classRecords,
                List<ItemMisc> stateRecords)
            {
                Records = records;
                CategoryRecords = categoryRecords;
                SubcategoryRecords = subcategoryRecords;
                ClassRecords = classRecords;
                StateRecords = stateRecords;
            }

            public DataResponse() { }
        }

        public async static Task<DataResponse> ImportCsv(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header?.Trim(),
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null, // avoids exception
            };

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return new DataResponse();

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            var records = new List<SerializableItem>();

            var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var subcategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var states = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                await foreach (var record in csv.GetRecordsAsync<SerializableItem>())
                {
                    record.ModelOrValue = (record.ModelOrValue ?? string.Empty).Trim();
                    record.Category = (record.Category ?? string.Empty).Trim();
                    record.Subcategory = (record.Subcategory ?? string.Empty).Trim();
                    record.Class = (record.Class ?? string.Empty).Trim();
                    record.State = (record.State ?? string.Empty).Trim();

                    records.Add(record);

                    if (!string.IsNullOrEmpty(record.Category)) categories.Add(record.Category);
                    if (!string.IsNullOrEmpty(record.Subcategory)) subcategories.Add(record.Subcategory);
                    if (!string.IsNullOrEmpty(record.Class)) classes.Add(record.Class);
                    if (!string.IsNullOrEmpty(record.State)) states.Add(record.State);
                }
            }
            catch (ReaderException ex)
            {
                StatusManager.Instance.UpdateMessageStatus($"Error al parsear el CSV: {ex}", StatusManager.MessageType.ERROR);
                
                // Return everything that got cached
                return new DataResponse(
                    records.Select(r => new StockItemDraft(r)).ToList(),
                    categories.Select(c => new ItemMisc(c)).ToList(),
                    subcategories.Select(c => new ItemMisc(c)).ToList(),
                    classes.Select(c => new ItemMisc(c)).ToList(),
                    states.Select(c => new ItemMisc(c)).ToList()
                );
            }
            catch (Exception ex)
            {
                StatusManager.Instance.UpdateMessageStatus($"Error inesperado al leer el CSV: {ex}", StatusManager.MessageType.ERROR);
                return new DataResponse(
                    records.Select(r => new StockItemDraft(r)).ToList(),
                    categories.Select(c => new ItemMisc(c)).ToList(),
                    subcategories.Select(c => new ItemMisc(c)).ToList(),
                    classes.Select(c => new ItemMisc(c)).ToList(),
                    states.Select(c => new ItemMisc(c)).ToList()
                );
            }

            return new DataResponse(
                records.Select(record => new StockItemDraft(record)).ToList(),
                categories.Select(c => new ItemMisc(c)).ToList(),
                subcategories.Select(c => new ItemMisc(c)).ToList(),
                classes.Select(c => new ItemMisc(c)).ToList(),
                states.Select(c => new ItemMisc(c)).ToList()
            );
        }

        readonly static ItemSubcategories subcategoryStorage = ItemSubcategories.Instance;

        readonly static ItemClasses classStorage = ItemClasses.Instance;
        readonly static ItemStates stateStorage = ItemStates.Instance;

        async static Task CreateCategoriesAsync(KeyValuePair<ItemMisc, ItemMisc[]> link, IDbTransaction transaction)
        {
            var elements = link.Value;

            foreach (var subcategory in elements)
            {
                var newId = await subcategoryStorage.AddAsync(subcategory, transaction).ConfigureAwait(false);
                subcategory.Id = newId;
            }

            var catId = await CategoryService.RegisterCategoryAsync(link.Key, elements.Select(subcat => subcat.Id), transaction).ConfigureAwait(false);
            link.Key.Id = catId;
        }

        async static Task CreateClassesAsync(KeyValuePair<ItemMisc, ItemMisc[]> link, IDbTransaction transaction)
        {
            var elements = link.Value;

            var newClassId = await classStorage.AddAsync(link.Key, transaction).ConfigureAwait(false);
            link.Key.Id = newClassId;

            foreach (var @class in elements)
            {
                var newStateId = await stateStorage.AddAsync(@class, newClassId, transaction).ConfigureAwait(false);
                @class.Id = newStateId;
            }
        }

        public static async Task SaveDataAsync(DataResponse model)
        {
            if (model == null) return;

            await using var initialConn = await DbConnection.CreateAndOpenAsync().ConfigureAwait(false);
            await using var transaction = initialConn.BeginTransaction();

            try
            {
                foreach (var link in model.LinkMap)
                {
                    switch (link.Key.Type)
                    {
                        case MiscType.Category:
                            await CreateCategoriesAsync(link, transaction).ConfigureAwait(false);
                            break;

                        case MiscType.Class:
                            await CreateClassesAsync(link, transaction).ConfigureAwait(false);
                            break;
                    }
                }

                await StockItems.Instance.AddRangeAsync(model.Records.Select(draft => draft.ToStockItem()), transaction).ConfigureAwait(false);

                transaction.Commit();

                await ItemSubcategories.Instance.LoadAsync().ConfigureAwait(false);
                await ItemCategories.Instance.LoadAsync().ConfigureAwait(false);
                await ItemClasses.Instance.LoadAsync().ConfigureAwait(false);
                await ItemStates.Instance.LoadAsync().ConfigureAwait(false);
                await StockItems.Instance.LoadAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rbEx)
                {
                    StatusManager.Instance.UpdateMessageStatus($"Error al deshacer los cambios hechos: {rbEx}", StatusManager.MessageType.ERROR);
                }

                await StatusManager.Instance.UpdateMessageStatusAsync($"Hubo un error al intentar importar los datos: {ex.Message}", StatusManager.MessageType.ERROR).ConfigureAwait(false);
            }
        }
    }
}
