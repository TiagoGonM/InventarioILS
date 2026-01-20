using InventarioILS.Model.Storage;
using System.ComponentModel;

namespace InventarioILS.Model
{
    public class ItemMisc : IIdentifiable
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Shorthand { get; set; }

        public ItemMisc() { }
        public ItemMisc(string name, string shorthand = null)
        {
            Name = name; Shorthand = shorthand;
        }
    }

    public class VisualItemMisc : INotifyPropertyChanged
    {
        public ItemMisc Model { get; }

        public VisualItemMisc(ItemMisc model)
        {
            Model = model;
        }

        public VisualItemMisc(string name, string shorthand = null)
        {
            Model = new ItemMisc(name, shorthand);
        }

        public string Name => Model.Name;

        public string Shorthand
        {
            get => Model.Shorthand;
            set
            {
                if (Model.Shorthand != value)
                {
                    Model.Shorthand = value;
                    OnPropertyChanged(nameof(Shorthand));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
