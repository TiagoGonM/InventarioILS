namespace InventarioILS.Model
{
    public abstract class Item : IIdentifiable
    {
        public int? Id { get; set; }
        public string ProductCode { get; set; }


        public string Category { get; set; }
        public int CategoryId { get; set; }


        public string Subcategory { get; set; }
        public int SubcategoryId { get; set; }


        public string Class { get; set; }
        public int ClassId { get; set; }
        public string Description { get; set; }
        
        protected Item(string productCode, int categoryId, int subcategoryId, string description, int classId)
        {
            ProductCode = productCode;
            CategoryId = categoryId;
            SubcategoryId = subcategoryId;
            Description = description;
            ClassId = classId;
        }

        protected Item() { }
    }

    public class StockItem : Item
    {
        public string State { get; set; }
        public int StateId { get; set; }
        public string Location { get; set; }
        public int Quantity { get; set; }
        public string AdditionalNotes { get; set; }
        
        public StockItem(
            string productCode,
            int categoryId,
            int subcategoryId,
            string description,
            int classId,
            int stateId,
            string location,
            int quantity,
            string additionalNotes = "") 
            : base(productCode, categoryId, subcategoryId, description, classId)
        {
            StateId = stateId;
            Location = location;
            Quantity = quantity;
            AdditionalNotes = additionalNotes;
        }

        public StockItem() { }
    }

    public class StockItemExtra : StockItem
    {
        public string ModelOrValue { get; set; }

        public StockItemExtra(
            string productCode,
            string modelOrVal,
            int categoryId,
            int subcategoryId,
            string description,
            int classId,
            int stateId,
            string location,
            int quantity,
            string additionalNotes = "")
            : base(productCode, categoryId, subcategoryId, description, classId, stateId, location, quantity, additionalNotes)
        {
            ModelOrValue = modelOrVal;
            StateId = stateId;
            Location = location;
            Quantity = quantity;
            AdditionalNotes = additionalNotes;
        }
    }

    public class OrderItem : Item
    {
        public string Name { get; set; }
        public string ShipmentState { get; set; }
        public int Quantity { get; set; }
    }
}
