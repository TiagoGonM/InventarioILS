using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventarioILS
{
    internal class Filter
    {
        public static string[] appliedFilters = new string[] { };
        
        public string name;

        public Filter(string name) {
            this.name = name;
        }
    }
}
