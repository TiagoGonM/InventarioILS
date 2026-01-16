using InventarioILS.Services;
using InventarioILS.View.Windows;
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
        }

        private async void ImportDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var data = await DataImportService.ImportCsv(Path.Combine(Directory.GetCurrentDirectory(), "data.csv"));

            new CSVImportWindow(data).ShowDialog();
        }
    }
}
