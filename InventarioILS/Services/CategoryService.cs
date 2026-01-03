using InventarioILS.Model;
using InventarioILS.Model.Storage;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventarioILS.Services
{
    internal class CategoryService
    {
        readonly static ItemCategories categories = ItemCategories.Instance;

        public async static Task RegisterCategory(ItemMisc category, IEnumerable<uint> subcategoryIds)
        {
            using var client = await DbConnection.CreateAndOpenAsync();
            using var transaction = client.BeginTransaction();

            await categories.AddAsync(category, transaction);

            try
            {
                foreach (uint subcatId in subcategoryIds)
                    await categories.LinkWithAsync(subcatId, client.LastRowIdInserted, transaction);
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
