using InventarioILS.Model;
using InventarioILS.Model.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventarioILS.Services
{
    internal class CategoryService
    {
        static ItemCategories categories = ItemCategories.Instance;

        public async static Task RegisterCategory(ItemMisc category, IEnumerable<uint> subcategoryIds)
        {
            await categories.AddAsync(category);

            foreach (uint subcatId in subcategoryIds)
            {
                await categories.LinkWithAsync(subcatId);
            }

            await categories.LoadAsync();
        }
    }
}
