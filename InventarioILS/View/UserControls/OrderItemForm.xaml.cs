using InventarioILS.Model;
using InventarioILS.Model.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InventarioILS.View.UserControls
{
    public partial class OrderItemForm : UserControl, IDisposable
    {
        readonly ItemCategories categoryStorage = ItemCategories.Instance;
        readonly ItemSubcategories subcategoryStorage = ItemSubcategories.Instance;
        readonly ItemClasses classStorage = ItemClasses.Instance;

        string codeMain = "";

        string ResultingProductCode { get; set; }

        bool isEditing = false;

        public event EventHandler<ItemEventArgs> OnConfirm;
        public event EventHandler<ItemEventArgs> OnConfirmEdit;

        OrderItem resultingItem = null;

        public OrderItemForm(OrderItem presetData = null)
        {
            InitializeComponent();

            if (presetData != null) PresetData = presetData;

            DataContext = new
            {
                CategoryList = categoryStorage.Items,
                SubcategoryList = subcategoryStorage.Items,
                ClassList = classStorage.Items,
                ResultingProductCode,
            };
        }

        public static readonly DependencyProperty PresetDataProperty =
            DependencyProperty.Register(
                nameof(PresetData),
                typeof(OrderItem),
                typeof(OrderItemForm),
                new PropertyMetadata(null, OnPresetDataChanged)
            );

        public OrderItem PresetData
        {
            get => (OrderItem)GetValue(PresetDataProperty);
            set { 
                SetValue(PresetDataProperty, value);
                FillData();
            }
        }

        public ItemMisc SelectedCategory => (ItemMisc)CategoryComboBox.SelectedItem;
        public ItemMisc SelectedSubcategory => (ItemMisc)SubcategoryComboBox.SelectedItem;
        public ItemMisc SelectedClass => (ItemMisc)ClassComboBox.SelectedItem;

        private void FillData()
        {
            if (PresetData == null) return;

            isEditing = true;
            ConfirmBtn.Content = "Guardar cambios";

            ProductCode.Text = PresetData.ProductCode;
            
            ExtraValueInput.Text = PresetData.ModelOrValue;
            if (!string.IsNullOrEmpty(ExtraValueInput.Text))
                ExtraValueCheckbox.IsChecked = true;
            
            CategoryComboBox.SelectedItemId = PresetData.CategoryId;
            SubcategoryComboBox.SelectedItemId = PresetData.SubcategoryId;
            ClassComboBox.SelectedItemId = PresetData.ClassId;
            
            QuantityInput.Text = PresetData.Quantity.ToString();

            DescriptionInput.Text = PresetData.Description;

            if (PresetData.CategoryId > 0) subcategoryStorage.Load(PresetData.CategoryId);

            ConfirmBtn.IsEnabled = true;
        }

        private static void OnPresetDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (OrderItemForm)d;
            control.PresetData = (OrderItem)e.NewValue;
        }

        // Como el Dispatcher ejecuta al setter de SelectedItemId de manera tardía (que tambien setea SelectedItem)
        // debemos usar ? para evitar errores por ser SelectedItem un posible nulo
        private void UpdateDescription()
        {
            DescriptionInput.Text = $"{SelectedCategory?.Name} {SelectedSubcategory?.Name} {((bool)ExtraValueCheckbox.IsChecked ? ExtraValueInput.Text.ToUpper() : "")}";
        }

        private void UpdateProductCode()
        {
            ProductCode.Text = $"{SelectedCategory?.Shorthand}-{codeMain}";
        }

        private void TryEnableSubmitBtn()
        {
            ConfirmBtn.IsEnabled =
                SelectedCategory?.Id > 0
                && SelectedSubcategory?.Id > 0
                && SelectedClass?.Id > 0;
        }

        private async void CategoryComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var category = (ItemMisc)combo.SelectedItem;

            if (category == null) return;

            await subcategoryStorage.LoadAsync(category.Id);
            SubcategoryComboBox.IsEnabled = true;

            UpdateProductCode();
            UpdateDescription();
            TryEnableSubmitBtn();
        }

        private void SubcategoryComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var subcat = (ItemMisc)combo.SelectedItem;

            if (subcat == null) return;

            if (!ExtraValueCheckbox.IsChecked.HasValue)
                codeMain = subcat.Name.ToUpper();

            UpdateProductCode();
            UpdateDescription();
            TryEnableSubmitBtn();
        }

        private void ClassComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var itemClass = (ItemMisc)combo.SelectedItem;

            if (itemClass == null) return;

            TryEnableSubmitBtn();
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCategory.Id == 0 || SelectedSubcategory.Id == 0 || SelectedClass.Id == 0)
                return;

            resultingItem = new OrderItem(
                productCode: ProductCode.Text,
                modelOrVal: ExtraValueInput.Text,
                categoryId: (uint)SelectedCategory.Id,
                subcategoryId: (uint)SelectedSubcategory.Id,
                classId: (uint)SelectedClass.Id,
                description: DescriptionInput.Text,
                quantity: uint.Parse(QuantityInput.Text)
            );

            ResultingProductCode = ProductCode.Text;

            if (!isEditing)
            {
                OnConfirm?.Invoke(this, new ItemEventArgs(resultingItem));
                return;
            }
            OnConfirmEdit?.Invoke(this, new ItemEventArgs(PresetData, resultingItem));
            
            isEditing = false;
            ConfirmBtn.Content = "Agregar elemento";
        }

        private void ExtraValueCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ExtraValueInput.IsEnabled = true;
            codeMain = ExtraValueInput.Text.Length > 0 ? ExtraValueInput.Text.ToUpper() : SelectedSubcategory.Name.ToUpper();
            
            UpdateProductCode();
            UpdateDescription();
            
            ExtraValueLabel.Foreground = (Brush)FindResource("AccentForegroundBrush");
        }

        private void ExtraValueCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ExtraValueInput.IsEnabled = false;
            codeMain = SelectedSubcategory.Name.ToUpper();

            UpdateProductCode();
            UpdateDescription();
            
            ExtraValueLabel.Foreground = SystemColors.GrayTextBrush;
        }

        private void ExtraValueInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            codeMain = ExtraValueInput.Text.Length > 0 ? ExtraValueInput.Text.ToUpper() : SelectedSubcategory.Name.ToUpper();

            UpdateProductCode();
            UpdateDescription();
        }

        private void QuantityInput_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void DummyBtn_Click(object sender, RoutedEventArgs e)
        {
            resultingItem = new OrderItem(
                productCode: "R-230K",
                modelOrVal: "230K",
                categoryId: 1,
                subcategoryId: 1,
                description: "Resistencia Estándar 230K",
                classId: 1,
                quantity: 10
            );

            OnConfirm?.Invoke(this, new ItemEventArgs(resultingItem));
        }

        public void Dispose()
        {
            OnConfirm = null;
            OnConfirmEdit = null;
            GC.SuppressFinalize(this);
        }
    }
}
