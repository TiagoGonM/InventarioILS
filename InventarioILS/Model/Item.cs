namespace InventarioILS.Model
{
    public abstract class Item : IIdentifiable
    {
        public int? Id { get; set; }
        public string ProductCode { get; set; }

        public string ModelOrValue { get; set; }

        public string Category { get; set; }
        public int CategoryId { get; set; }

        public string Subcategory { get; set; }
        public int SubcategoryId { get; set; }

        public string Class { get; set; }
        public int ClassId { get; set; }
        
        public string Description { get; set; }

        public int Quantity { get; set; }
        
        protected Item(string productCode, int categoryId, int subcategoryId, int classId, string description, int quantity = 1, string modelOrVal = null)
        {
            ProductCode = productCode;
            ModelOrValue = modelOrVal;
            CategoryId = categoryId;
            SubcategoryId = subcategoryId;
            ClassId = classId;
            Description = description;
            Quantity = quantity;
        }

        protected Item() { }
    }

    public class StockItem : Item
    {
        public string State { get; set; }
        public int StateId { get; set; }
        public string Location { get; set; }
        public string AdditionalNotes { get; set; }
        
        public StockItem(
            string productCode,
            int categoryId,
            int subcategoryId,
            int classId,
            int stateId,
            string description,
            string location,
            int quantity,
            string additionalNotes = "", 
            string modelOrVal = null)
            : base(productCode, categoryId, subcategoryId, classId, description, quantity, modelOrVal)
        {
            StateId = stateId;
            Location = location;
            Quantity = quantity;
            AdditionalNotes = additionalNotes;
        }

        public StockItem() { }
    }

    public class OrderItem : Item
    {
        public string ShipmentState { get; set; }
        public int ShipmentStateId { get; set; }

        public OrderItem() { }
        public OrderItem(string productCode, int categoryId, int subcategoryId, int shipmentStateId, int classId, string description, int quantity) 
            : base(productCode, categoryId, subcategoryId, classId, description, quantity)
        {
            ShipmentStateId = shipmentStateId;
        }
    }
}
