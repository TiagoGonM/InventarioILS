using CsvHelper;
using CsvHelper.Configuration;
using InventarioILS.Model;
using InventarioILS.Model.Serializables;
using InventarioILS.Model.Storage;
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
            public List<SerializableItem> Records { get; set; }
            public IEnumerable<ItemMisc> CategoryRecords { get; set; }
            public IEnumerable<ItemMisc> SubcategoryRecords { get; set; }
            public IEnumerable<ItemMisc> ClassRecords { get; set; }
            public IEnumerable<ItemMisc> StateRecords { get; set; }

            public DataResponse(
                List<SerializableItem> records,
                IEnumerable<ItemMisc> categoryRecords,
                IEnumerable<ItemMisc> subcategoryRecords,
                IEnumerable<ItemMisc> classRecords,
                IEnumerable<ItemMisc> stateRecords)
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
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            var records = new List<SerializableItem>();

            var categories = new HashSet<string>();
            var subcategories = new HashSet<string>();
            var classes = new HashSet<string>();
            var states = new HashSet<string>();

            await foreach (var record in csv.GetRecordsAsync<SerializableItem>())
            {
                records.Add(record);

                categories.Add(record.Category.Trim());
                subcategories.Add(record.Subcategory.Trim());
                classes.Add(record.Class.Trim());
                states.Add(record.State.Trim());
            }

            return new DataResponse(
                records, 
                categories.Select(c => new ItemMisc(c)),
                subcategories.Select(c => new ItemMisc(c)),
                classes.Select(c => new ItemMisc(c)),
                states.Select(c => new ItemMisc(c))
            );
        }
    }
}
