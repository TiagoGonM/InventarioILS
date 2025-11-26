using InventarioILS.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        readonly ItemStates states = null;

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
            states = ItemStates.Instance;

            DataContext = new
            {
                CategoryList = categories.Items,
                SubcategoryList = subCategories.Items,
                ClassList = classes.Items,
                StateList = states.Items,
            };
        }

        // Evento personalizado OnConfirm
        public event EventHandler<StockItemEventArgs> OnConfirm;

        private void UpdateDescription()
        {
            DescriptionInput.Text = $"{selectedCategory} {selectedSubcategory} {((bool)ExtraValueCheckbox.IsChecked ? ExtraValueInput.Text.ToUpper() : "")}";
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

            CodeMain.Text = subcat.Name.ToUpper();

            selectedSubcategory = subcat.Name;
            selectedSubcategoryId = subcat.Id;

            UpdateDescription();
        }

        private void ClassComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var itemClass = (ItemMisc)combo.SelectedItem;

            if (itemClass == null) return;

            //StateComboBox.IsEnabled = itemClass.Name != "Insumo";

            selectedClass = itemClass.Name;
            selectedClassId = itemClass.Id;
        }

        private void StateComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var itemState = (ItemMisc)combo.SelectedItem;

            if (itemState == null) return;

            selectedState = itemState.Name;
            selectedStateId = itemState.Id;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCategoryId <= -1 || selectedSubcategoryId <= -1 || selectedClassId <= -1)
                return;

            // Crear la instancia de StockItem con los datos del formulario
            var stockItem = new StockItem(
                productCode: $"{CodeCategoryShorthand.Text}-{CodeMain.Text}",
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

        private void ExtraValueCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ExtraValueInput.IsEnabled = true;
            CodeMain.Text = ExtraValueInput.Text.Length > 0 ? ExtraValueInput.Text.ToUpper() : selectedSubcategory.ToUpper();
            UpdateDescription();
            ExtraValueLabel.Foreground = (Brush)FindResource("AccentForegroundBrush");
        }

        private void ExtraValueCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ExtraValueInput.IsEnabled = false;
            CodeMain.Text = selectedSubcategory.ToUpper();
            UpdateDescription();
            ExtraValueLabel.Foreground = SystemColors.GrayTextBrush;
        }

        private void ExtraValueInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CodeMain.Text = ExtraValueInput.Text.Length > 0 ? ExtraValueInput.Text.ToUpper() : selectedSubcategory.ToUpper();
            UpdateDescription();
        }

        private void QuantityInput_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
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
