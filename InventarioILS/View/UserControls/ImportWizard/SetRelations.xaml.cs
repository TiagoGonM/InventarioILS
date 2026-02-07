using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.Services;
using InventarioILS.View.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static InventarioILS.Services.DataImportService;

namespace InventarioILS.View.UserControls.ImportWizard
{
    public partial class SetRelations : UserControl, IWizardStep
    {
        public VisualItemMisc SelectedCategory => (VisualItemMisc)CategoryListBox.SelectedItem;
        public List<VisualItemMisc> SelectedSubcategories => [.. SubcategoryListBox.SelectedItems.Cast<VisualItemMisc>()];

        public VisualItemMisc SelectedClass => (VisualItemMisc)ClassListBox.SelectedItem;
        public List<VisualItemMisc> SelectedStates => [.. StateListBox.SelectedItems.Cast<VisualItemMisc>()];

        readonly Map<ItemMisc, uint[]> _linkMap = [];

        private uint _catSubcatlinkCount = 0;
        private uint _classStateLinkCount = 0;

        public SetRelations()
        {
            InitializeComponent();
        }

        public SetRelations(DataResponse data) : this()
        {

            DataContext = new
            {
                CategoryList = data.CategoryRecords.Select(item => new VisualItemMisc(item)).ToObservableCollection(),
                SubcategoryList = data.SubcategoryRecords.Select(item => new VisualItemMisc(item)).ToObservableCollection(),

                ClassList = data.ClassRecords.Select(item => new VisualItemMisc(item)).ToObservableCollection(),
                StateList = data.StateRecords.Select(item => new VisualItemMisc(item)).ToObservableCollection(),
            };
        }

        private void CatSubcatLinkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCategory.LinkIds.Length == 0)
            {
                _catSubcatlinkCount++;
                SelectedCategory.SetLink(_catSubcatlinkCount);
            }

            foreach (var subcategory in SelectedSubcategories)
            {
                subcategory.AddLink(_catSubcatlinkCount);
            }

            _linkMap.AddOrUpdate(SelectedCategory.Model, [.. SelectedSubcategories.Select(subcategory => subcategory.Id)]);

            MessageBox.Show($"Linking {SelectedCategory.Name} category with {SelectedSubcategories.Count} subcategories.");
        }
        private void ClassStatesLinkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClass.LinkIds.Length == 0)
            {
                _classStateLinkCount++;
                SelectedClass.SetLink(_classStateLinkCount);
            }

            foreach (var state in SelectedStates)
            {
                state.AddLink(_classStateLinkCount);
            }

            _linkMap.AddOrUpdate(SelectedClass.Model, [.. SelectedStates.Select(state => state.Id)]);

            MessageBox.Show($"Linking {SelectedClass.Name} class with {SelectedStates.Count} states.");
        }

        public bool Validate()
        {
            //if (!(_data.CategoryRecords.All(category => _linkMap.ContainsKey(category)) && _linkMap.All(link => link.Value.Length > 0)))
            //{
            //    result = null;
            //    return false;
            //}

            //foreach (var link in _linkMap)
            //{
            //    CategoryService.RegisterCategoryAsync(link.Key, link.Value);
            //}

            return true;
        }

    }
}
