using InventarioILS.Model.Serializables;
using InventarioILS.Model.Storage;
using InventarioILS.View.UserControls.ImportWizard;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using static InventarioILS.Services.DataImportService;

namespace InventarioILS.View.Windows
{
    /// <summary>
    /// Lógica de interacción para CSVImportWindow.xaml
    /// </summary>
    public partial class CSVImportWindow : Window
    {
        public IEnumerable<SerializableItem> Items { get; set; }
        public string Categories { get; set; }
        public string Subcategories { get; set; }
        public string Classes { get; set; }
        public string States { get; set; }

        public CSVImportWindow()
        {
            InitializeComponent();
        }

        public CSVImportWindow(DataResponse data) : this()
        {
            Items = data.Records.ToObservableCollection();
            Categories = string.Join(", ", data.CategoryRecords);
            Subcategories = string.Join(", ", data.SubcategoryRecords);
            Classes = string.Join(", ", data.ClassRecords);
            States = string.Join(", ", data.StateRecords);

            DataContext = new
            {
                Items,
                Categories,
                Subcategories,
                Classes,
                States
            };

            WizardControl.Content = new SetShorthands(data.CategoryRecords, data.SubcategoryRecords);
        }
    }
}
