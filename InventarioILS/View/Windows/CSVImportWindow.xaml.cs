using InventarioILS.Model.Serializables;
using InventarioILS.Model.Storage;
using InventarioILS.View.UserControls.ImportWizard;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using static InventarioILS.Services.DataImportService;

namespace InventarioILS.View.Windows
{
    /// <summary>
    /// Lógica de interacción para CSVImportWindow.xaml
    /// </summary>
    public partial class CSVImportWindow : Window
    {
        // 0?. Posibilidad de mapear columnas pre importación para evitar errores?
        // 1. Setear shorthands para categorías y subcategorías
        // 2. Relación categoría-subcategoría | A lo mejor relación clase-estado
        // 3. Previsualización final
        public enum WizardSteps
        {
            STEP1,
            STEP2,
            STEP3,
        }

        public IEnumerable<SerializableItem> Items { get; set; }
        public string Categories { get; set; }
        public string Subcategories { get; set; }
        public string Classes { get; set; }
        public string States { get; set; }

        public CSVImportWindow()
        {
            InitializeComponent();
        }

        private void SetWindow(WizardSteps step, DataResponse data)
        {
            UserControl content = null;

            switch (step)
            {
                case WizardSteps.STEP1:
                    content = new SetShorthands(data.CategoryRecords, data.SubcategoryRecords);
                    break;

                case WizardSteps.STEP2:
                    // content = 
                    break;
            }

            if (content != null)
            {
                WizardControl.Content = content;
            }
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

            SetWindow(WizardSteps.STEP1, data);

        }

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NextPageBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PreviousPageBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
