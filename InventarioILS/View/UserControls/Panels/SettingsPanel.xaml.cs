using InventarioILS.Services;
using InventarioILS.View.Windows;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls.Panels
{
    /// <summary>
    /// Lógica de interacción para SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : UserControl
    {
        public SettingsPanel()
        {
            InitializeComponent();

            AppVersion.Text = $"Versión: {App.AppVersion}";
        }

        private async void ImportDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv",

                Title = "Seleccionar archivo CSV para importar"
            };

            Nullable<bool> result = dlg.ShowDialog();

            string filename;

            if (result == true) filename = dlg.FileName;
            else return;

            var data = await DataImportService.ImportCsv(filename);

            new CSVImportWindow(data).ShowDialog();
        }

        private void CreateCategory_Click(object sender, RoutedEventArgs e)
        {
            new NewCategoryWindow().ShowDialog();
        }

        private void CreateSubcategory_Click(object sender, RoutedEventArgs e)
        {
            new NewSubcategoryWindow().ShowDialog();
        }

        private void CreateClass_Click(object sender, RoutedEventArgs e)
        {
            new NewClassWindow().ShowDialog();
        }

        private void CreateState_Click(object sender, RoutedEventArgs e)
        {
            new NewStateWindow().ShowDialog();
        }

        private void CreateShipmentState_Click(object sender, RoutedEventArgs e)
        {
            new NewShipmentStateWindow().ShowDialog();
        }
    }
}
