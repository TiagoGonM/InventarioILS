using InventarioILS.Model;
using InventarioILS.Model.Storage;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace InventarioILS.Services
{
    public class CategoryService
    {
        readonly static ItemCategories categories = ItemCategories.Instance;

        public async static Task<uint> RegisterCategoryAsync(ItemMisc category, IEnumerable<uint> subcategoryIds, IDbTransaction transaction)
        {
            uint id = await categories.AddAsync(category, transaction);

            foreach (uint subcatId in subcategoryIds)
                await categories.LinkWithAsync(subcatId, id, transaction);

            return id;
        }
    }
}
