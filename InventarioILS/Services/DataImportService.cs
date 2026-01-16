using CsvHelper;
using CsvHelper.Configuration;
using InventarioILS.Model;
using InventarioILS.Model.Serializables;
using InventarioILS.Model.Storage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace InventarioILS.Services
{
    public class DataImportService
    {
        public class DataResponse(
            List<SerializableItem> records, 
            HashSet<string> categoryRecords, 
            HashSet<string> subcategoryRecords, 
            HashSet<string> classRecords, 
            HashSet<string> stateRecords)
        {
            public List<SerializableItem> Records { get; } = records;
            public HashSet<string> CategoryRecords { get; } = categoryRecords;
            public HashSet<string> SubcategoryRecords { get; } = subcategoryRecords;
            public HashSet<string> ClassRecords { get; } = classRecords;
            public HashSet<string> StateRecords { get; } = stateRecords;
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

            //var records = csv.GetRecords<SerializableItem>();
            //var collection = records.ToObservableCollection();

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

            return new DataResponse(records, categories, subcategories, classes, states);
        }
    }
}
