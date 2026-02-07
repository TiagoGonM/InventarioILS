using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.Services;
using InventarioILS.View.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static InventarioILS.Services.DataImportService;

namespace InventarioILS.View.UserControls.ImportWizard
{
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

        public SetShorthands(DataResponse data) : this()
        {

            CategoryList = data.CategoryRecords.Select(cat => new VisualItemMisc(cat)).ToObservableCollection();
            SubcategoryList = data.SubcategoryRecords.Select(subcat => new VisualItemMisc(subcat)).ToObservableCollection();

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

        public bool Validate()
        {
            //if (!CategoryList.All(c => !string.IsNullOrWhiteSpace(c.Shorthand)))
            //{
            //    result = null;
            //    return false;
            //}

            foreach (var subcategory in SubcategoryList)
            {
                uint newId = ItemSubCategories.Instance.Add(subcategory.Model);
                subcategory.Id = newId;
            }
            
            return true;
        }
    }
}
