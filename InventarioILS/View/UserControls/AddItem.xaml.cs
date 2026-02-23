using InventarioILS.Model;
using System;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para AddItem.xaml
    /// </summary>
    public partial class AddItem : UserControl
    {
        readonly ItemCategories categories = null;
        readonly ItemSubCategories subCategories = null;
        readonly ItemClasses classes = null;
        readonly ItemState state = null;

        string selectedCategory = "";
        int selectedCategoryId = -1;

        string selectedSubcategory = "";
        int selectedSubcategoryId = -1;

        string selectedClass = "";
        int selectedClassId = -1;

        string selectedState = "";
        int selectedStateId = -1;

        public AddItem()
        {
            InitializeComponent();

            categories = ItemCategories.Instance;
            subCategories = ItemSubCategories.Instance;
            classes = ItemClasses.Instance;
            state = ItemState.Instance;

            DataContext = new
            {
                CategoryList = categories.Items,
                SubcategoryList = subCategories.Items,
                ClassList = classes.Items,
                StateList = state.Items,
            };
        }

        public event EventHandler<StockItemEventArgs> OnConfirm;

        private void UpdateDescription()
        {
            DescriptionInput.Text = $"{selectedCategory} {selectedSubcategory} {ExtraValueInput.Text} {QuantityInput.Text}";
        }

        private void CategoryComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var category = (Category)combo.SelectedItem;

            if (category == null) return;

            CodeCategoryShorthand.Text = category.Shorthand;

            // No se puede asignar la instancia directamente 💔
            selectedCategory = category.Name;
            selectedCategoryId = category.Id;

            UpdateDescription();
        }

        private void SubcategoryComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var subcat = (ItemMisc)combo.SelectedItem;

            if (subcat == null) return;

            selectedSubcategory = subcat.Name;
            selectedSubcategoryId = subcat.Id;

            if (ExtraValueInput.Text == "") CodeMain.Text = subcat.Name.ToUpper();

            UpdateDescription();
        }

        private void ClassComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var itemClass = (ItemMisc)combo.SelectedItem;

            if (itemClass == null) return;

            selectedClass = itemClass.Name;
            selectedClassId = itemClass.Id;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCategoryId <= -1 || selectedSubcategoryId <= -1 || selectedClassId <= -1 || selectedStateId <= -1)
                return;

            // Crear la instancia de StockItem con los datos del formulario
            var stockItem = new StockItem(
                productCode: $"{CodeCategoryShorthand}-{CodeMain}",
                categoryId: selectedCategoryId,
                subcategoryId: selectedSubcategoryId,
                description: DescriptionInput.Text,
                classId: selectedClassId,
                stateId: selectedStateId,
                location: LocationInput.Text,
                quantity: int.Parse(QuantityInput.Text),
                additionalNotes: NotesInput.Text
            );

            // Disparar el evento OnConfirm
            OnConfirm?.Invoke(this, new StockItemEventArgs(stockItem));
        }

        private void ExtraValueCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ExtraValueInput.IsEnabled = true;
            CodeMain.Text = ExtraValueInput.Text.ToUpper();
        }

        private void ExtraValueCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ExtraValueInput.IsEnabled = false;
            CodeMain.Text = selectedSubcategory.ToUpper();
        }

        private void StateComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var state = (ItemMisc)combo.SelectedItem;

            if (state == null) return;

            selectedState = state.Name;
            selectedStateId = state.Id;
        }

        private void ExtraValueInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ExtraValueInput.Text == "")
            {
                CodeMain.Text = selectedSubcategory.ToUpper();
                return;
            }
            
            CodeMain.Text = ExtraValueInput.Text.ToUpper();
        }
    }

    /// <summary>
    /// Clase personalizada para pasar datos del evento OnConfirm
    /// </summary>
    public class StockItemEventArgs(StockItem stockItem) : EventArgs
    {
        public StockItem Item { get; } = stockItem;
    }
}
