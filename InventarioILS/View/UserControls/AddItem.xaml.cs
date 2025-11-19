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

        string selectedCategory = "";
        int selectedCategoryId = -1;

        string selectedSubcategory = "";
        int selectedSubcategoryId = -1;

        string selectedClass = "";
        int selectedClassId = -1;

        public AddItem()
        {
            InitializeComponent();

            categories = ItemCategories.Instance;
            subCategories = ItemSubCategories.Instance;
            classes = ItemClasses.Instance;

            DataContext = new
            {
                CategoryList = categories.Items,
                SubcategoryList = subCategories.Items,
                ClassList = classes.Items,
            };
        }

        // Evento personalizado OnConfirm
        public event EventHandler<StockItemEventArgs> OnConfirm;

        private void UpdateDescription()
        {
            DescriptionInput.Text = $"{selectedCategory} {selectedSubcategory}";
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
            if (selectedCategoryId <= -1 || selectedSubcategoryId <= -1 || selectedClassId <= -1)
                return;

            // Crear la instancia de StockItem con los datos del formulario
            var stockItem = new StockItem(
                productCode: "R-500K",
                categoryId: selectedCategoryId,
                subcategoryId: selectedSubcategoryId,
                description: DescriptionInput.Text,
                classId: selectedClassId,
                stateId: 1,
                location: "",
                quantity: 1,
                additionalNotes: NotesInput.Text
            );
            // Disparar el evento OnConfirm
            OnConfirm?.Invoke(this, new StockItemEventArgs(stockItem));
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
