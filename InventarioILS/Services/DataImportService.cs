using CsvHelper;
using CsvHelper.Configuration;
using InventarioILS.Model;
using InventarioILS.Model.Serializables;
using InventarioILS.Model.Storage;
using InventarioILS.Model.Wizard;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace InventarioILS.Services
{
    public class DataImportService
    {
        public class DataResponse {
            public List<StockItemDraft> Records { get; set; }
            public List<ItemMisc> CategoryRecords { get; set; }
            public List<ItemMisc> SubcategoryRecords { get; set; }
            public List<ItemMisc> ClassRecords { get; set; }
            public List<ItemMisc> StateRecords { get; set; }

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
            // TODO
            // 1. Recuperar todos los datos iniciales (categorias, subcategorias, clases, estados)
            // 2. Crear items a partir del CSV

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim(),
                TrimOptions = TrimOptions.Trim,
            };

            if (!File.Exists(filePath))
                return null;

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            var records = new List<SerializableItem>();

            var categories = new HashSet<string>();
            var subcategories = new HashSet<string>();
            var classes = new HashSet<string>();
            var states = new HashSet<string>();

            await foreach (var record in csv.GetRecordsAsync<SerializableItem>())
            {
                record.ModelOrValue = record.ModelOrValue.Trim();

                records.Add(record);

                categories.Add(record.Category.Trim());
                subcategories.Add(record.Subcategory.Trim());
                classes.Add(record.Class.Trim());
                states.Add(record.State.Trim());
            }

            return new DataResponse(
                [.. records.Select(record => new StockItemDraft(record))],
                [.. categories.Select(c => new ItemMisc(c))],
                [.. subcategories.Select(c => new ItemMisc(c))],
                [.. classes.Select(c => new ItemMisc(c))],
                [.. states.Select(c => new ItemMisc(c))]
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
                var newId = await subcategoryStorage.AddAsync(subcategory, transaction);
                subcategory.Id = newId;
            }

            var catId = await CategoryService.RegisterCategoryAsync(link.Key, elements.Select(subcat => subcat.Id), transaction);
            link.Key.Id = catId;
        }

        async static Task CreateClassesAsync(KeyValuePair<ItemMisc, ItemMisc[]> link, IDbTransaction transaction)
        {
            var elements = link.Value;

            var newClassId = await classStorage.AddAsync(link.Key, transaction);
            link.Key.Id = newClassId;

            foreach (var @class in elements)
            {
                var newStateId = await stateStorage.AddAsync(@class, newClassId, transaction);
                @class.Id = newStateId;
            }
        }

        public static async Task SaveDataAsync(DataResponse model)
        {
            // 1. Crear y guardar categorias, subcategorias, clases y estados en la BD
            // 2. Crear y guardar los items en la BD

            if (model == null) return;

            using var initialConn = await DbConnection.CreateAndOpenAsync();
            var transaction = initialConn.BeginTransaction();

            try
            {
                foreach (var link in model.LinkMap)
                {
                    switch (link.Key.Type)
                    {
                        case MiscType.Category:
                            await CreateCategoriesAsync(link, transaction);
                            break;

                        case MiscType.Class:
                            await CreateClassesAsync(link, transaction);
                            break;
                    }
                }

                await StockItems.Instance.AddRangeAsync(model.Records.Select(draft => draft.ToStockItem()), transaction);

                transaction.Commit();

                await ItemSubcategories.Instance.LoadAsync().ConfigureAwait(false);
                await ItemCategories.Instance.LoadAsync().ConfigureAwait(false);
                await ItemClasses.Instance.LoadAsync().ConfigureAwait(false);
                await ItemStates.Instance.LoadAsync().ConfigureAwait(false);
                await StockItems.Instance.LoadAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                await StatusManager.Instance.UpdateMessageStatusAsync($"Hubo un error al intentar importar los datos: {ex.Message}", StatusManager.MessageType.ERROR);
            }
        }
    }
}
