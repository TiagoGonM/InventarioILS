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
        string selectedSubcategory = "";
        string selectedClass = "";

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

        private void CategoryComboBox_SelectedItemChanged(object sender, System.EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var category = (Category)combo.SelectedItem;

            if (category == null) return;

            CodeCategoryShorthand.Text = category.Shorthand;

            selectedCategory = category.Name;

            UpdateDescription();
        }

        private void SubcategoryComboBox_SelectedItemChanged(object sender, System.EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var subcat = (ItemMisc)combo.SelectedItem;

            if (subcat == null) return;

            selectedSubcategory = subcat.Name;

            UpdateDescription();
        }

        private void ClassComboBox_SelectedItemChanged(object sender, System.EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var itemClass = (ItemMisc)combo.SelectedItem;

            if (itemClass == null) return;

            selectedClass = itemClass.Name;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCategory) || string.IsNullOrEmpty(selectedSubcategory) || string.IsNullOrEmpty(selectedClass))
                return;

            // Crear la instancia de StockItem con los datos del formulario
            var stockItem = new StockItem(
                productCode: "",
                categoryName: selectedCategory,
                subcategoryName: selectedSubcategory,
                description: DescriptionInput.Text,
                type: selectedClass,
                state: "",
                location: "",
                quantity: 0,
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
