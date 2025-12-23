using InventarioILS.Model.Storage;

namespace InventarioILS.Model
{
    public class Item : IIdentifiable
    {
        public uint Id { get; set; }
        public string ProductCode { get; set; }

        public string ModelOrValue { get; set; }

        public string Category { get; set; }
        public uint CategoryId { get; set; }

        public string Subcategory { get; set; }
        public uint SubcategoryId { get; set; }

        public string Class { get; set; }
        public uint ClassId { get; set; }
        
        public string Description { get; set; }

        public uint Quantity { get; set; }
        
        protected Item(string productCode, uint categoryId, uint subcategoryId, uint classId, string description, uint quantity = 1, string modelOrVal = null)
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
        public int LocalIndexTag { get; set; }
        public string State { get; set; }
        public uint StateId { get; set; }
        public string Location { get; set; }
        public string AdditionalNotes { get; set; }
        
        public StockItem(
            string productCode,
            uint categoryId,
            uint subcategoryId,
            uint classId,
            uint stateId,
            string description,
            string location,
            uint quantity,
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
        public uint? OrderId { get; set; }
        public uint? ItemId { get; set; }

        public int LocalIndexTag { get; set; }
        public string ShipmentState { get; set; }
        public uint? ShipmentStateId { get; set; }

        public OrderItem(string productCode, uint categoryId, uint subcategoryId, uint classId, string description, uint quantity, string modelOrVal = null, uint? shipmentStateId = null) 
            : base(productCode, categoryId, subcategoryId, classId, description, quantity, modelOrVal)
        {
            ShipmentStateId = shipmentStateId;
        }

        public OrderItem(uint? orderId, uint? itemId, uint? shipmentStateId, uint quantity)
        {
            OrderId = orderId;
            ItemId = itemId;
            ShipmentStateId = shipmentStateId;
            Quantity = quantity;
        }

        public OrderItem() { }
    }
}
