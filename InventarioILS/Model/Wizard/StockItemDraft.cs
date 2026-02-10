using InventarioILS.Model.Serializables;
using InventarioILS.Services;
using System.ComponentModel;

namespace InventarioILS.Model.Wizard
{
    public class StockItemDraft : INotifyPropertyChanged
    {
        public SerializableItem Source { get; set; }

        public ItemMisc CategoryRef { get; set; }
        public ItemMisc SubcategoryRef { get; set; }

        public ItemMisc ClassRef { get; set; }
        public ItemMisc StateRef { get; set; }

        public string ProductCode => ItemService.GenerateProductCode(CategoryRef, SubcategoryRef, Source.ModelOrValue);
        public string Description => ItemService.GenerateDescription(Source, CategoryRef, SubcategoryRef);

        public StockItemDraft(SerializableItem source)
        {
            Source = source;
        }

        public StockItem ToStockItem() => new(this);

        public bool IsDevice => string.Equals(ClassRef.Name, "dispositivo", System.StringComparison.OrdinalIgnoreCase);

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
