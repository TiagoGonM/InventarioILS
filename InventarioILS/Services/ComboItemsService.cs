using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.View.Windows;
using System;
using static InventarioILS.View.UserControls.QueryableComboBox;

namespace InventarioILS.Services
{
    internal class ComboItemsService
    {
        public static void HandleCreation(ComboTags tag)
        {
            switch (tag)
            {
                case ComboTags.Category:
                    new NewCategoryWindow().ShowDialog();
                    break;
                case ComboTags.Subcategory:
                    new NewSubcategoryWindow().ShowDialog();
                    break;
                case ComboTags.Class:
                    new NewClassWindow().ShowDialog();
                    break;
                case ComboTags.State:
                    new NewStateWindow().ShowDialog();
                    break;
                case ComboTags.ShipmentState:
                    new NewShipmentStateWindow().ShowDialog();
                    break;
                default:
                    throw new ArgumentNullException("Error de aplicación: el elemento no posee ninguna tag, por lo que no es posible de crear.");
            }
        }

        public async static void HandleDeletion(ComboTags tag, uint id)
        {
            DeleteResult result = null;

            switch (tag)
            {
                case ComboTags.Category:
                    result = await ItemCategories.Instance.DeleteAsync(id);
                    break;
                case ComboTags.Subcategory:
                    result = await ItemSubCategories.Instance.DeleteAsync(id);
                    break;
                case ComboTags.Class:
                    result = await ItemClasses.Instance.DeleteAsync(id);
                    break;
                case ComboTags.State:
                    result = await ItemStates.Instance.DeleteAsync(id);
                    break;
                case ComboTags.ShipmentState:
                    result = await ShipmentStates.Instance.DeleteAsync(id);
                    break;
                default:
                    throw new ArgumentNullException("Error de aplicación: el elemento no posee ninguna tag, por lo que no es posible de eliminar.");
            }

            if (result is null) return;

            if (result.Success)
            {
                await StatusManager.Instance.UpdateMessageStatusAsync(result.Message, StatusManager.MessageType.SUCCESS);
                return;
            }
            await StatusManager.Instance.UpdateMessageStatusAsync(result.Message, StatusManager.MessageType.ERROR);
        }
    }
}
