using InventarioILS.Model;
using InventarioILS.Model.Serializables;
using InventarioILS.Model.Storage;
using InventarioILS.Services;
using InventarioILS.View.UserControls.ImportWizard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static InventarioILS.Services.DataImportService;

namespace InventarioILS.View.Windows
{
    public interface IWizardStep
    {
        //DataResponse GetData();
        bool Validate();
    }

    public partial class CSVImportWindow : Window
    {
        // 0?. Posibilidad de mapear columnas pre importación para evitar errores?
        // 1. Setear shorthands para categorías y subcategorías
        // 2. Relación categoría-subcategoría | A lo mejor relación clase-estado
        // 3. Previsualización final
        public enum WizardSteps
        {
            STEP1 = 0,
            STEP2 = 1,
            STEP3 = 2,
        }

        IWizardStep CurrentControl { get; set; }

        public WizardSteps CurrentStep => (WizardSteps)_currentStepInd;
        private uint _currentStepInd = 0;

        static DataResponse Data { get; set; }

        public CSVImportWindow()
        {
            InitializeComponent();
        }

        private void SetWindow()
        {
            SubmitBtn.IsEnabled = false;


            switch (CurrentStep)
            {
                case WizardSteps.STEP1:
                    CurrentControl = new SetShorthands(Data);
                    break;

                case WizardSteps.STEP2:
                    CurrentControl = new SetRelations(Data);
                    break;

                case WizardSteps.STEP3:
                    CurrentControl = new ImportPreview(Data);
                    SubmitBtn.IsEnabled = true;
                    break;
            }

            if (CurrentControl != null)
            {
                WizardControl.Content = CurrentControl;
            }
            LinkReferences();
        }

        private void Next()
        {
            _currentStepInd++;

            int limit = System.Enum.GetValues<WizardSteps>().Length;

            if (_currentStepInd >= limit)
            {
                _currentStepInd = (uint)(limit - 1);
                NextPageBtn.IsEnabled = false;
            }
            PreviousPageBtn.IsEnabled = true;
        }

        private void Previous()
        {
            _currentStepInd--;
            NextPageBtn.IsEnabled = true;

            if (_currentStepInd < 0)
            {
                _currentStepInd = 0;
                PreviousPageBtn.IsEnabled = false;
            }
        }

        public void LinkReferences()
        {
            foreach (var itemDraft in Data.Records)
            {
                itemDraft.CategoryRef = Data.CategoryRecords.FirstOrDefault(cat => cat.Name == itemDraft.Source.Category);
                itemDraft.SubcategoryRef = Data.SubcategoryRecords.FirstOrDefault(subcat => subcat.Name == itemDraft.Source.Subcategory);
                itemDraft.ClassRef = Data.ClassRecords.FirstOrDefault(type => type.Name == itemDraft.Source.Class);
                itemDraft.StateRef = Data.StateRecords.FirstOrDefault(state => state.Name == itemDraft.Source.State);
            }
        }

        public CSVImportWindow(DataResponse data) : this()
        {
            if (data is null) return;

            Data = data;
            
            SetWindow();
        }

        private async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentControl is ImportPreview control)
            {
                foreach (var itemDraft in Data.Records)
                {
                    MessageBox.Show(
                        $"Item: {itemDraft.ProductCode}\n" +
                        $"Categoría: {itemDraft.CategoryRef?.Name ?? "null"} : {itemDraft.CategoryRef?.Shorthand ?? "null"}\n" +
                        $"Subcategoría: {itemDraft.SubcategoryRef?.Name ?? "null"} : {itemDraft.SubcategoryRef?.Shorthand ?? "null"}\n" +
                        $"Clase: {itemDraft.ClassRef?.Name ?? "null"} : {itemDraft.ClassRef?.Shorthand ?? "null"}\n" +
                        $"Estado: {itemDraft.StateRef?.Name ?? "null"} : {itemDraft.StateRef?.Shorthand ?? "null"}");

                    //var newItem = new StockItem
                    //{
                    //    Name = itemDraft.Source.Name,
                    //    Description = itemDraft.Source.Description,
                    //    Quantity = itemDraft.Source.Quantity,
                    //    Category = itemDraft.CategoryRef,
                    //    Subcategory = itemDraft.SubcategoryRef,
                    //    Location = itemDraft.Source.Location,
                    //    State = itemDraft.Source.State,
                    //    Class = itemDraft.Source.Class,
                    //};

                    break;
                }

                //await StockItems.Instance.AddRangeAsync(items);

                //MessageBox.Show("Importación finalizada con éxito");
                //Close();
            }
        }

        private void NextPageBtn_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(string.Join(", ", CurrentControl.GetData().CategoryRecords.Select(x => x.Shorthand)));
            var success = CurrentControl.Validate();

            if (!success) return;
            //if (!success)
            //{
            //    MessageBox.Show("Debes asignarle una abreviatura a todas las categorías");
            //    return;
            //}

            // 2. Tomamos el primer registro como muestra
            var sample = Data.Records.FirstOrDefault();
            if (sample != null)
            {
                // Buscamos manualmente si el nombre existe en la lista de categorías
                var categoryInList = Data.CategoryRecords.FirstOrDefault(c => c.Name == sample.Source.Category);

                // EL MOMENTO DE LA VERDAD:
                bool sonElMismoObjeto = Object.ReferenceEquals(sample.CategoryRef, categoryInList);

                MessageBox.Show(
                    $"¿CategoryRef es null?: {sample.CategoryRef == null}\n" +
                    $"¿Existe en la lista maestra?: {categoryInList != null}\n" +
                    $"¿Son la misma instancia de memoria?: {sonElMismoObjeto}");
            }

            Next();
            SetWindow();
        }

        private void PreviousPageBtn_Click(object sender, RoutedEventArgs e)
        {
            Previous();
            SetWindow();
        }
    }
}
