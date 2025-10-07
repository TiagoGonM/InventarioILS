namespace InventarioILS
{
    public enum Class
    {
        INSUMO,
        DISPOSITIVO,
        REPUESTO
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

    public class IItem
    {
        string ProductCode { get; set; }
        string CategoryName { get; set; }
        string SubcategoryName { get; set; }
        Class Class { get; set; }
        string Description { get; set; }
    }

    public class StockItem : IItem
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
        {
            ProductCode = productCode;
            CategoryName = categoryName;
            SubcategoryName = subcategoryName;
            Description = description;
            Class = type;
            State = state;
            Location = location;
            Quantity = quantity;
            AdditionalNotes = additionalNotes;
        }

        public StockItem(string productCode, string description, Class type, string state, string location, int quantity, string additionalNotes = "") { 
            ProductCode = productCode;
            Description = description;
            Class = type;
            State = state;
            Location = location;
            Quantity = quantity;
            AdditionalNotes = additionalNotes;
        }

        public StockItem() { }

        public string ProductCode { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public Class Class { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Quantity { get; set; }
        public string AdditionalNotes { get; set; }
    }

    public class OrderItem : IItem
    {
        public string ProductCode { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public Class Class { get; set; }
        public string Description { get; set; }
    }
}
