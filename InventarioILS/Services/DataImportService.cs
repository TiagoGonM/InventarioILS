using CsvHelper;
using CsvHelper.Configuration;
using InventarioILS.Model;
using InventarioILS.Model.Serializables;
using InventarioILS.Model.Storage;
using InventarioILS.Model.Wizard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;

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

        public static async void SaveData(DataResponse model) 
        {
            // 1. Crear y guardar categorias, subcategorias, clases y estados en la BD
            // 2. Crear y guardar los items en la BD

            if (model == null) return;

            var classStorage = ItemClasses.Instance;
            var stateStorage = ItemStates.Instance;

            //CategoryService.RegisterCategoryAsync();

            //foreach (var draft in model.Records)
            //{
            //    var item = new StockItem(...);
            //}
        }
    }
}
