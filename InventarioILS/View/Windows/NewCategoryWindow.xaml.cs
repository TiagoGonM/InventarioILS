using InventarioILS.Model.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InventarioILS.View.Windows
{
    public partial class NewCategoryWindow : Window
    {
        readonly ItemSubCategories subcategories = new();

        public NewCategoryWindow()
        {
            InitializeComponent();

            DataContext = new
            {
                SubcategoryList = subcategories.Items
            };
        }
    }
}
