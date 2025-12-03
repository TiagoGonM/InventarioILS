using InventarioILS.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InventarioILS.View.UserControls
{
    public partial class OrderItemForm : UserControl
    {
        readonly ItemCategories categories = null;
        readonly ItemSubCategories subCategories = null;
        readonly ItemClasses classes = null;
        readonly ItemStates states = null;

        string codeCategoryShorthand = "";
        string codeMain = "";

        string selectedCategory = "";
        int selectedCategoryId = -1;

        string selectedSubcategory = "";
        int selectedSubcategoryId = -1;

        string selectedClass = "";
        int selectedClassId = -1;

        string selectedState = "";
        int selectedStateId = -1;

        string ResultingProductCode { get; set; }

        bool isEditing = false;

        public event EventHandler<StockItemEventArgs> OnConfirm;
        public event EventHandler<StockItemEventArgs> OnEdit;

        StockItem resultingItem = null;

        public OrderItemForm()
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
                ResultingProductCode,
            };
        }

        public static readonly DependencyProperty PresetDataProperty =
            DependencyProperty.Register(
                nameof(PresetData),
                typeof(StockItem),
                typeof(OrderItemForm),
                new PropertyMetadata(null, OnPresetDataChanged)
            );

        public StockItem PresetData
        {
            get => (StockItem)GetValue(PresetDataProperty);
            set { 
                SetValue(PresetDataProperty, value);
                FillData();
            }
        }

        private static void SetComboBoxItem<T>(QueryableComboBox combo, ObservableCollection<T> filterList, int predicate) where T : IIdentifiable
        {
            foreach (var item in filterList)
            {
                var it = (T)item;

                if (it.Id == predicate)
                    combo.SelectedItem = it;
            }
        }

        private void FillData()
        {
            if (PresetData == null) return;

            isEditing = true;
            ConfirmBtn.Content = "Guardar cambios";

            ProductCode.Text = PresetData.ProductCode;
            
            ExtraValueInput.Text = PresetData.ModelOrValue;
            if (ExtraValueInput.Text != null || ExtraValueInput.Text != "")
                ExtraValueCheckbox.IsChecked = true;

            SetComboBoxItem<Category>(CategoryComboBox, categories.Items, PresetData.CategoryId);
            SetComboBoxItem<ItemMisc>(SubcategoryComboBox, subCategories.Items, PresetData.SubcategoryId);
            SetComboBoxItem<ItemMisc>(ClassComboBox, classes.Items, PresetData.ClassId);

            QuantityInput.Text = PresetData.Quantity.ToString();

            DescriptionInput.Text = PresetData.Description;
            NotesInput.Text = PresetData.AdditionalNotes;
        }

        private static void OnPresetDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (OrderItemForm)d;
            control.PresetData = (StockItem)e.NewValue;
        }

        private void UpdateDescription()
        {
            DescriptionInput.Text = $"{selectedCategory} {selectedSubcategory} {((bool)ExtraValueCheckbox.IsChecked ? ExtraValueInput.Text.ToUpper() : "")}";
        }

        private void UpdateProductCode()
        {
            ProductCode.Text = $"{codeCategoryShorthand}-{codeMain}";
        }

        private void CategoryComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var category = (Category)combo.SelectedItem;

            if (category == null) return;

            codeCategoryShorthand = category.Shorthand;

            // No se puede asignar la instancia directamente 💔
            selectedCategory = category.Name;
            selectedCategoryId = (int)category.Id;


            UpdateProductCode();
            UpdateDescription();
        }

        private void SubcategoryComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var subcat = (ItemMisc)combo.SelectedItem;

            if (subcat == null) return;

            if (ExtraValueCheckbox.IsChecked == false)
                codeMain = subcat.Name.ToUpper();

            selectedSubcategory = subcat.Name;
            selectedSubcategoryId = (int)subcat.Id;

            UpdateProductCode();
            UpdateDescription();
        }

        private void ClassComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var itemClass = (ItemMisc)combo.SelectedItem;

            if (itemClass == null) return;

            //StateComboBox.IsEnabled = itemClass.Name != "Insumo";

            selectedClass = itemClass.Name;
            selectedClassId = (int)itemClass.Id;
        }

        private void StateComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var itemState = (ItemMisc)combo.SelectedItem;

            if (itemState == null) return;

            selectedState = itemState.Name;
            selectedStateId = (int)itemState.Id;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCategoryId <= -1 || selectedSubcategoryId <= -1 || selectedClassId <= -1)
                return;

            //resultingItem = new OrderItem(
            //    productCode: ProductCode.Text,
            //    //modelOrVal: ExtraValueInput.Text,
            //    categoryId: selectedCategoryId,
            //    subcategoryId: selectedSubcategoryId,
            //    description: DescriptionInput.Text,
            //    classId: selectedClassId,
            //    quantity: int.Parse(QuantityInput.Text),
            //    additionalNotes: NotesInput.Text
            //);

            //ProductCodeAfterConfirmLabel.Text = code;
            ResultingProductCode = ProductCode.Text;

            if (!isEditing)
            {
                OnConfirm?.Invoke(this, new StockItemEventArgs(resultingItem));
                return;
            }
            OnEdit?.Invoke(this, new StockItemEventArgs(PresetData, resultingItem));
            isEditing = false;
            ConfirmBtn.Content = "Agregar elemento";
        }

        private void ExtraValueCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ExtraValueInput.IsEnabled = true;
            codeMain = ExtraValueInput.Text.Length > 0 ? ExtraValueInput.Text.ToUpper() : selectedSubcategory.ToUpper();
            
            UpdateProductCode();
            UpdateDescription();
            
            ExtraValueLabel.Foreground = (Brush)FindResource("AccentForegroundBrush");
        }

        private void ExtraValueCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ExtraValueInput.IsEnabled = false;
            codeMain = selectedSubcategory.ToUpper();

            UpdateProductCode();
            UpdateDescription();
            
            ExtraValueLabel.Foreground = SystemColors.GrayTextBrush;
        }

        private void ExtraValueInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            codeMain = ExtraValueInput.Text.Length > 0 ? ExtraValueInput.Text.ToUpper() : selectedSubcategory.ToUpper();

            UpdateProductCode();
            UpdateDescription();
        }

        private void QuantityInput_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void DummyBtn_Click(object sender, RoutedEventArgs e)
        {
            resultingItem = new StockItem(
                productCode: "R-230K",
                modelOrVal: "230K",
                categoryId: 2,
                subcategoryId: 3,
                description: "Resistencia Estándar 230K",
                classId: 1,
                stateId: 2,
                location: "Cajón 3",
                quantity: 10,
                additionalNotes: ""
            );

            OnConfirm?.Invoke(this, new StockItemEventArgs(resultingItem));
        }
    }
}
