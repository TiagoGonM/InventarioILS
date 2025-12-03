using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventarioILS.Model
{
    internal class ItemMisc : IIdentifiable
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    }
    
    internal class Category : ItemMisc
    {
        public string Shorthand { get; set; }
    }

}
