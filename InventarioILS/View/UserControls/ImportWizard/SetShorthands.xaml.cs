using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.Services;
using InventarioILS.View.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Lógica de interacción para SetShorthands.xaml
    /// </summary>
    public partial class SetShorthands : UserControl, IWizardStep
    {
        public ObservableCollection<VisualItemMisc> CategoryList { get; set; } = [];
        public ObservableCollection<VisualItemMisc> SubcategoryList { get; set; } = [];
        public VisualItemMisc SelectedCategory => (VisualItemMisc)CategoryListBox.SelectedItem;
        public VisualItemMisc SelectedSubcategory => (VisualItemMisc)SubcategoryListBox.SelectedItem;

        public SetShorthands()
        {
            InitializeComponent();
        }

        public SetShorthands(IEnumerable<ItemMisc> categories, IEnumerable<ItemMisc> subcategories) : this()
        {
            CategoryList = categories.Select(name => new VisualItemMisc(name)).ToObservableCollection();
            SubcategoryList = subcategories.Select(name => new VisualItemMisc(name)).ToObservableCollection();

            DataContext = new
            {
                CategoryList,
                SubcategoryList,
            };
        }

        private void CategoryShorthandSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedCategory.Shorthand = CategoryShorthandInput.Text.Trim();

            CategoryShorthandForm.Visibility = Visibility.Collapsed;
            CategoryShorthandInput.Text = "";
        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CategoryShorthandInput.Text = SelectedCategory.Shorthand;

            CategoryShorthandForm.Visibility = Visibility.Visible;
        }

        private void SubcategoryShorthandSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedSubcategory.Shorthand = SubcategoryShorthandInput.Text.Trim();

            SubcategoryShorthandForm.Visibility = Visibility.Collapsed;
            CategoryShorthandInput.Text = "";
        }

        private void SubcategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SubcategoryShorthandInput.Text = SelectedSubcategory.Shorthand;

            SubcategoryShorthandForm.Visibility = Visibility.Visible;
        }

        public DataImportService.DataResponse GetData()
        {
            return new DataImportService.DataResponse
            {
                CategoryRecords = CategoryList.Select(c => c.Model),
                SubcategoryRecords = SubcategoryList.Select(s => s.Model),
            };
        }
    }
}
