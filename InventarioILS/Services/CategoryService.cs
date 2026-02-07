using InventarioILS.Model;
using InventarioILS.Model.Storage;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventarioILS.Services
{
    public class CategoryService
    {
        readonly static ItemCategories categories = ItemCategories.Instance;

        public async static Task RegisterCategoryAsync(ItemMisc category, IEnumerable<uint> subcategoryIds)
        {
            using var client = await DbConnection.CreateAndOpenAsync();
            using var transaction = client.BeginTransaction();

            uint id = await categories.AddAsync(category, transaction);

            try
            {
                foreach (uint subcatId in subcategoryIds)
                    await categories.LinkWithAsync(subcatId, id, transaction);
            }
            catch (SqliteException)
            {
                transaction.Rollback();
                throw;
            }

            transaction.Commit();
            await categories.LoadAsync();
        }
    }
}
