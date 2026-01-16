using InventarioILS.Model;
using InventarioILS.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using static InventarioILS.View.UserControls.QueryableComboBox;

namespace InventarioILS
{
    public partial class App : Application
    {
        private async void NewComboBoxBtn_Click(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;

            if (e.OriginalSource is Button btn && btn.Name == "AddNewItem")
            {
                try
                {
                    ComboItemsService.HandleCreation((ComboTags)combo.Tag);
                } catch (ArgumentNullException ex)
                {
                    await StatusManager.Instance.UpdateMessageStatusAsync(ex.Message, StatusManager.MessageType.ERROR);
                }

                e.Handled = true;
            }
        }
    }
}
