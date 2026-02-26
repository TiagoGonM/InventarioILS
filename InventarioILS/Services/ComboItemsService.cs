using InventarioILS.Model;
using InventarioILS.Model.Storage;
using InventarioILS.View.Windows;
using System;
using System.Configuration;
using System.Windows;
using static InventarioILS.View.UserControls.QueryableComboBox;

namespace InventarioILS.Services
{
    public class ComboItemsService
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

            var confirm = MessageBox.Show("¿Está seguro de que desea eliminar este elemento? Esta acción no se puede deshacer.", "Confirmar eliminación", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (confirm.HasFlag(MessageBoxResult.Cancel) && !confirm.HasFlag(MessageBoxResult.OK)) return;

            switch (tag)
            {
                case ComboTags.Category:
                    result = await ItemCategories.Instance.DeleteAsync(id);
                    break;
                case ComboTags.Subcategory:
                    result = await ItemSubcategories.Instance.DeleteAsync(id);
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
                case ComboTags.Default:
                    MessageBox.Show("El botón no se encuentra asignado a ningun comportamiento. Por favor, intente desde los ajustes de la aplicación.");
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
