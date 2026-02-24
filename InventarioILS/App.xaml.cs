using InventarioILS.Model;
using InventarioILS.Services;
using System;
using System.Reflection;
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

        public static string AppVersion
        {
            get
            {
                string revisionId = Assembly.GetExecutingAssembly()
                                 .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                 .InformationalVersion ?? "Debug";

                if (revisionId.Contains('+'))
                {
                    var spl = revisionId.Split("+");
                    string version = spl[0];
                    string hash = spl[1];

                    string shortHash = hash.Length > 7 ? hash[..7] : hash;

                    return $"v{version} (dev | {shortHash})";
                }

                return $"v{revisionId}";
            }
        }
    }
}
