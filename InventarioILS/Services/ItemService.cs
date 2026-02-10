using Dapper;
using InventarioILS.Model;
using InventarioILS.Model.Serializables;
using InventarioILS.Model.Storage;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioILS.Services
{
    public class ItemService
    {
        public async static Task<uint> AddItemAsync(Item item, IDbTransaction transaction)
        {
            var conn = transaction.Connection;
            int catSubcatId;

            try
            {
                //catSubcatId = await conn.QuerySingleOrDefaultAsync<int>(
                //    @"SELECT catSubcatId FROM CatSubcat
                //      WHERE categoryId = @CategoryId 
                //      AND subcategoryId = @SubcategoryId",
                //    new { item.CategoryId, item.SubcategoryId },
                //    transaction
                //);

                catSubcatId = await ItemCategories.Instance.EnsureCatSubcatIdAsync(item.CategoryId, item.SubcategoryId, conn, transaction);

                if (catSubcatId == default) // Si no se encontró el ID
                {
                    throw new InvalidOperationException(
                        $"No se encontró CatSubcatId para CategoryId: {item.CategoryId}, SubcategoryId: {item.SubcategoryId}");
                }
            }
            catch (SqliteException ex)
            {
                throw new ApplicationException("Error al buscar CatSubcatId: " + ex.Message);
            }

            //if (!string.IsNullOrEmpty(item.Class) && string.Equals(item.Class, "dispositivo", StringComparison.OrdinalIgnoreCase))
            //{
            //    string queryMax = @"
            //        SELECT MAX(CAST(SUBSTR(productCode, LENGTH(@BaseCode) + 2) AS INTEGER))
            //        FROM Item
            //        WHERE productCode LIKE @Pattern";

            //    var maxSuffix = await conn.QuerySingleOrDefaultAsync<int?>(
            //        queryMax,
            //        new { BaseCode = item.ProductCode, Pattern = $"{item.ProductCode}-%" },
            //        transaction);

            //    // 2. Generamos el código con sufijo (ej: T-UT39-1)
            //    int nextSuffix = (maxSuffix ?? 0) + 1;
            //    item.ProductCode = $"{item.ProductCode}-{nextSuffix}";
            //}

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
                transaction
            );

            return rowId;
        }

        public static async Task<uint> CountByProductCodeAsync(string productCode)
        {
            using var conn = await DbConnection.CreateAndOpenAsync();
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

        public static string GenerateProductCode(ItemMisc category, ItemMisc subcategory, string productModelOrExtraVal = null)
        {
            if (category == null || subcategory == null) return "";

            if (productModelOrExtraVal == null)
                return $"{category.Shorthand}-{(string.IsNullOrEmpty(subcategory.Shorthand) ? subcategory.Name : subcategory.Shorthand)}";


            return $"{category.Shorthand}-{productModelOrExtraVal}";
        }

        public static string GenerateProductCode(ItemMisc category, string productModelOrExtraVal)
        {
            if (category == null || string.IsNullOrEmpty(productModelOrExtraVal)) return "";

            return $"{category.Shorthand}-{productModelOrExtraVal.ToUpper()}";
        }

        public static string GenerateDescription(SerializableItem item, ItemMisc category, ItemMisc subcategory)
        {
            if (category == null || subcategory == null) return "";

            return $"{category.Name} {subcategory.Name} {item.ModelOrValue ?? ""}";
        }
    }
}
