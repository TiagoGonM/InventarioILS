using InventarioILS.View.Windows;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS
{
    public partial class App : Application
    {
        private void NewComboBoxBtn_Click(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;

            if (e.OriginalSource is Button btn && btn.Name == "AddNewItem")
            {
                switch (combo.Tag)
                {
                    case "Category":
                        new NewCategoryWindow().ShowDialog();
                        break;
                    case "Subcategory":
                        new NewSubcategoryWindow().ShowDialog();
                        break;
                    case "Class":
                        MessageBox.Show("Good puppy");
                        break;
                    case "State":
                        MessageBox.Show("Good kitty");
                        break;
                    default:
                        MessageBox.Show("Not handled");
                        break;
                }

                e.Handled = true;
            }
        }
    }
}
