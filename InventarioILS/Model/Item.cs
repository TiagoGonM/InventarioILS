namespace InventarioILS.Model
{
    public enum Class
    {
        INSUMO = 1,
        REPUESTO = 2,
        DISPOSITIVO = 3
    }

    public enum State
    {
        DEVICE_FUNCIONAL = 6,
        DEVICE_REPARAR = 7,
        DEVICE_DESCARTAR = 8,
        
        INSUMO_SUFICIENTE = 1,
        INSUMO_ACOMPRAR = 2,
        
        REPUESTO_DISPONIBLE = 3,
        REPUESTO_USADO = 4,
        REPUESTO_DESCARTAR = 5 
    }

    public abstract class Item : IIdentifiable
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public Class Class { get; set; }
        public string Description { get; set; }
        
        protected Item(string productCode, string categoryName, string subcategoryName, string description, Class type)
        {
            ProductCode = productCode;
            CategoryName = categoryName;
            SubcategoryName = subcategoryName;
            Description = description;
            Class = type;
        }

        protected Item() { }
    }

    public class StockItem : Item
    {
        public StockItem(
            string productCode,
            string categoryName,
            string subcategoryName,
            string description,
            Class type,
            string state,
            string location,
            int quantity,
            string additionalNotes = "") 
            : base(productCode, categoryName, subcategoryName, description, type)
        {
            State = state;
            Location = location;
            Quantity = quantity;
            AdditionalNotes = additionalNotes;
        }

        public StockItem() { }

        public string State { get; set; }
        public string Location { get; set; }
        public int Quantity { get; set; }
        public string AdditionalNotes { get; set; }
    }

    public class OrderItem : Item
    {
        public string Name { get; set; }
        public string ShipmentState { get; set; }
        public int Quantity { get; set; }
    }
}
