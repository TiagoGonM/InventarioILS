using InventarioILS.Model;
using InventarioILS.Services;
using InventarioILS.View.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls.ImportWizard
{
    public partial class ImportPreview : UserControl, IWizardStep
    {
        public ImportPreview()
        {
            InitializeComponent();
        }

        public ImportPreview(DataImportService.DataResponse data) : this()
        {
            DataContext = new
            {
                Items = data.Records
            };
        }

        public bool Validate()
        {
            return true;
        }
    }
}
