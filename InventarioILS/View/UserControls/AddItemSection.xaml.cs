using InventarioILS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para AddItemSection.xaml
    /// </summary>
    public partial class AddItemSection : UserControl
    {
        readonly ItemCategories categories = null;
        readonly ItemSubCategories subCategories = null;
        readonly ItemClasses classes = null;

        public AddItemSection()
        {
            InitializeComponent();

            categories = new ItemCategories();
            subCategories = new ItemSubCategories();
            classes = new ItemClasses();

            categories.Load();
            subCategories.Load();
            classes.Load();

            this.DataContext = new
            {
                CategoryList = categories.Items,
                SubcategoryList = subCategories.Items,
                ClassList = classes.Items
            };
        }
    }
}
