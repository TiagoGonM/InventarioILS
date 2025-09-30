using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventarioILS.Model
{
    public class ItemModel
    {
        public int ItemId { get; set; }
        public string ProductCode { get; set; }
        public int CatSubcatId { get; set; }
        public string Class { get; set; }
        public string Description { get; set; }
    }

    public class StockItemModel : ItemModel
    {
        public string State { get; set; }
        public int Quantity { get; set; }
        public string Location { get; set; }
        public string AdditionalNotes { get; set; }
    }
}
