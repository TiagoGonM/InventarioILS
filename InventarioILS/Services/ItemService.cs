using Dapper;
using InventarioILS.Model;
using InventarioILS.Model.Storage;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Services
{
    internal class ItemService
    {
        public async static Task<uint> AddItemAsync(Item item, IDbTransaction transaction = null)
        {
            var conn = transaction?.Connection ?? new DbConnection();
            int catSubcatId;

            try
            {
                catSubcatId = await conn.QuerySingleOrDefaultAsync<int>(
                    @"SELECT catSubcatId FROM CatSubcat
                      WHERE categoryId = @CategoryId 
                      AND subcategoryId = @SubcategoryId COLLATE NOCASE",
                    new { item.CategoryId, item.SubcategoryId },
                    transaction: transaction
                ).ConfigureAwait(false);

                if (catSubcatId == default) // Si no se encontró el ID
                {
                    throw new InvalidOperationException(
                        $"No se encontró CatSubcatId para CategoryId: {item.CategoryId}, SubcategoryId: {item.SubcategoryId}");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al buscar CatSubcatId.", ex);
            }

            string insertSql =
                SQLUtils.IncludeLastRowIdInserted(
                  @"INSERT INTO Item (productCode, catSubcatId, classId, description)
                  VALUES (@ProductCode, @CatSubcatId, @ClassId, @Description)"
                );

            var parameters = new
            {
                item.ProductCode,
                CatSubcatId = catSubcatId,
                item.ClassId,
                item.Description,
            };

            uint rowId = await conn.ExecuteScalarAsync<uint>(
                insertSql,
                parameters,
                transaction: transaction
            ).ConfigureAwait(false);

            return rowId;
        }

        public static async Task<uint> CountByProductCodeAsync(string productCode)
        {
            using var conn = new DbConnection();
            var classStorage = ItemClasses.Instance;

            var deviceId = classStorage.Items.First((itemClass) => 
                itemClass.Name.Equals("dispositivo", StringComparison.OrdinalIgnoreCase)).Id;

            var count = await conn.QueryFirstAsync<uint>(
                @"SELECT COUNT(*) 
                  FROM Item
                  WHERE classId = @ClassId 
                    AND isDeleted = 0
                    AND (
                      productCode = @ProductCode 
                      OR productCode GLOB (@ProductCode || '-[0-9]*')
                    )",
            new { ProductCode = productCode, ClassId = deviceId });

            return count;
        }
    }
}
