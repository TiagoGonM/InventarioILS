using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InventarioILS.View.UserControls
{
    public class ItemFormPresetData(
        StockItem item, 
        bool enableCategory = true, 
        bool enableSubcategory = true, 
        bool enableClass = true, 
        bool enableState = true, 
        bool enableLocation = true, 
        bool enableAdditionalNotes = true)
    {
        public StockItem Data { get; set; } = item;

        public bool enableCategory = enableCategory;
        public bool enableSubcategory = enableSubcategory;
        public bool enableClass = enableClass;
        public bool enableState = enableState;
        public bool enableLocation = enableLocation;
        public bool enableAdditionalNotes = enableAdditionalNotes;
    }

    public partial class ItemForm : UserControl, IDisposable
    {
        readonly ItemCategories categories = ItemCategories.Instance;
        readonly ItemSubCategories subCategories = ItemSubCategories.Instance;
        readonly ItemClasses classes = ItemClasses.Instance;
        readonly ItemStates states = ItemStates.Instance;

        string codeMain = "";

        bool isDevice = false;
        uint deviceCounter = 0;

        string ResultingProductCode { get; set; }

        bool isEditing = false;

        public event EventHandler<ItemEventArgs> OnConfirm;
        public event EventHandler<ItemEventArgs> OnConfirmEdit;

        StockItem resultingItem = null;

        public ItemForm(ItemFormPresetData presetData = null)
        {
            InitializeComponent();

            if (presetData != null) PresetData = presetData;

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
                typeof(ItemFormPresetData),
                typeof(ItemForm),
                new PropertyMetadata(null, OnPresetDataChanged)
            );

        public ItemFormPresetData PresetData
        {
            get => (ItemFormPresetData)GetValue(PresetDataProperty);
            set { 
                SetValue(PresetDataProperty, value);
                FillData();
            }
        }

        
        public ItemMisc SelectedCategory => (ItemMisc)CategoryComboBox.SelectedItem;
        public ItemMisc SelectedSubcategory => (ItemMisc)SubcategoryComboBox.SelectedItem;
        public ItemMisc SelectedClass => (ItemMisc)ClassComboBox.SelectedItem;
        public ItemMisc SelectedState => (ItemMisc)StateComboBox.SelectedItem;


        private void FillData()
        {
            if (PresetData == null) return;

            isEditing = true;
            ConfirmBtn.Content = "Guardar cambios";

            var item = PresetData.Data;

            ProductCode.Text = item.ProductCode;

            CategoryComboBox.SelectedItemId = item.CategoryId;
            SubcategoryComboBox.SelectedItemId = item.SubcategoryId;
            ClassComboBox.SelectedItemId = item.ClassId;
            StateComboBox.SelectedItemId = item.StateId;
            
            ExtraValueInput.Text = item.ModelOrValue;
            if (!string.IsNullOrEmpty(ExtraValueInput.Text))
                ExtraValueCheckbox.IsChecked = true;


            QuantityInput.Text = item.Quantity.ToString();
            LocationInput.Text = item.Location ?? "";

            DescriptionInput.Text = item.Description ?? "";
            NotesInput.Text = item.AdditionalNotes ?? "";

            CategoryComboBox.IsEnabled = PresetData.enableCategory;
            SubcategoryComboBox.IsEnabled = PresetData.enableSubcategory;
            ClassComboBox.IsEnabled = PresetData.enableClass;
            StateComboBox.IsEnabled = PresetData.enableState;
            LocationInput.IsEnabled = PresetData.enableLocation;
            NotesInput.IsEnabled = PresetData.enableAdditionalNotes;
        }

        private static void OnPresetDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ItemForm)d;
            control.PresetData = (ItemFormPresetData)e.NewValue;
        }

        private void UpdateDescription()
        {
            DescriptionInput.Text = $"{SelectedCategory?.Name} {SelectedSubcategory?.Name} {(ExtraValueCheckbox.IsChecked.Value ? ExtraValueInput.Text.ToUpper() : "")}";
        }

        private void UpdateProductCode()
        {
            bool shouldShowUnitCount = isDevice && ExtraValueCheckbox.IsChecked.Value;

            ProductCode.Text = 
                $"{SelectedCategory?.Shorthand}-{codeMain}{(shouldShowUnitCount ? $"-{deviceCounter + 1}" : "")}";
        }

        private async Task CheckForDevice()
        {
            if (!isDevice) return;

            string baseCode = $"{SelectedCategory?.Shorthand}-{codeMain}";
            deviceCounter = await ItemService.CountByProductCodeAsync(baseCode);
        }

        private void CategoryComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var category = (ItemMisc)combo.SelectedItem;

            if (category == null) return;

            UpdateProductCode();
            UpdateDescription();
        }

        private void SubcategoryComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var subcat = (ItemMisc)combo.SelectedItem;

            if (subcat == null) return;

            if (!ExtraValueCheckbox.IsChecked.Value)
                codeMain = subcat.Name.ToUpper();

            UpdateProductCode();
            UpdateDescription();
        }

        private async void ClassComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var itemClass = (ItemMisc)combo.SelectedItem;

            if (itemClass == null) return;

            isDevice = itemClass.Name.Equals("dispositivo", StringComparison.OrdinalIgnoreCase);
            QuantityInput.IsEnabled = !isDevice;
            QuantityInput.Text = isDevice ? "1" : QuantityInput.Text;

            await CheckForDevice();
            UpdateProductCode();
        }

        private void StateComboBox_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var combo = (QueryableComboBox)sender;

            var itemState = (ItemMisc)combo.SelectedItem;

            if (itemState == null) return;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCategory.Id == 0 || SelectedSubcategory.Id == 0 || SelectedClass.Id == 0)
                return;

            resultingItem = new StockItem {
                ProductCode = ProductCode.Text,
                ModelOrValue = ExtraValueInput.Text,
                CategoryId = (uint)SelectedCategory.Id,
                SubcategoryId = (uint)SelectedSubcategory.Id,
                Description = DescriptionInput.Text,
                Class = SelectedClass.Name,
                ClassId = (uint)SelectedClass.Id,
                StateId = (uint)SelectedState.Id,
                Location = LocationInput.Text,
                Quantity = uint.Parse(QuantityInput.Text),
                AdditionalNotes = NotesInput.Text
            };

            ResultingProductCode = ProductCode.Text;

            if (!isEditing)
            {
                OnConfirm?.Invoke(this, new ItemEventArgs(resultingItem));
                return;
            }
            OnConfirmEdit?.Invoke(this, new ItemEventArgs(PresetData.Data, resultingItem));
            isEditing = false;
            ConfirmBtn.Content = "Agregar elemento";
        }

        private void ExtraValueCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ExtraValueInput.IsEnabled = true;
            codeMain = ExtraValueInput.Text.Length > 0 ? ExtraValueInput.Text.ToUpper() : SelectedSubcategory.Name.ToUpper() ?? "";
            
            UpdateProductCode();
            UpdateDescription();
            
            ExtraValueLabel.Foreground = (Brush)FindResource("AccentForegroundBrush");
        }

        private void ExtraValueCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ExtraValueInput.IsEnabled = false;
            codeMain = SelectedSubcategory.Name.ToUpper() ?? "";

            UpdateProductCode();
            UpdateDescription();
            
            ExtraValueLabel.Foreground = SystemColors.GrayTextBrush;
        }

        private async void ExtraValueInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            codeMain = ExtraValueInput.Text.Length > 0 ? ExtraValueInput.Text.ToUpper() : SelectedSubcategory.Name.ToUpper() ?? "";

            await CheckForDevice();
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
                categoryId: 1,
                subcategoryId: 1,
                description: "Resistencia Estándar 230K",
                classId: 1,
                stateId: 2,
                location: "Cajón 3",
                quantity: 10
            );

            OnConfirm?.Invoke(this, new ItemEventArgs(resultingItem));
        }

        public void Dispose()
        {
            OnConfirm = null;
            OnConfirmEdit = null;
        }
    }

    public class ItemEventArgs : EventArgs
    {
        public enum EventType
        {
            CREATE,
            EDIT,
            DELETE
        }

        public Item OldItem { get; }
        public Item Item { get; }
        public EventType Type { get; set; }

        public ItemEventArgs(Item item, EventType eventType = EventType.CREATE)
        {
            Item = item;
            Type = eventType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldItem">has to be filled when editing an item</param>
        /// <param name="item"></param>
        /// <param name="eventType"></param>
        public ItemEventArgs(Item oldItem, Item item, EventType eventType = EventType.EDIT)
        {
            OldItem = oldItem;
            Item = item;
            Type = eventType;
        }
    }
}
