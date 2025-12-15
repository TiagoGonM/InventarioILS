using InventarioILS.Model.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventarioILS.Model
{
    internal class ItemMisc : IIdentifiable
    {
        public uint Id { get; set; }
        public string Name { get; set; }

        public string Shorthand { get; set; }
    }
}
