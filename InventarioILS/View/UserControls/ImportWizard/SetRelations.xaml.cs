using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.Services;
using InventarioILS.View.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventarioILS.View.UserControls.ImportWizard
{
    /// <summary>
    /// Lógica de interacción para SetRelations.xaml
    /// </summary>
    public partial class SetRelations : UserControl, IWizardStep
    {
        public IList SelectedCategories => CategoryListBox.SelectedItems;
        public IList SelectedSubcategories => SubcategoryListBox.SelectedItems;

        public SetRelations()
        {
            InitializeComponent();
        }

        public SetRelations(IEnumerable<ItemMisc> categories, IEnumerable<ItemMisc> subcategories) : this()
        {
            DataContext = new
            {
                CategoryList = categories.Select(item => new VisualItemMisc(item)).ToObservableCollection(),
                SubcategoryList = subcategories.Select(item => new VisualItemMisc(item)).ToObservableCollection(),
            };
        }

        private void LinkBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Linking {SelectedCategories.Count} categories with {SelectedSubcategories.Count} subcategories.");
        }

        public DataImportService.DataResponse GetData()
        {
            throw new NotImplementedException();
        }
    }
}
