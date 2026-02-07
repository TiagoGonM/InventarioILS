using InventarioILS.Model.Storage;
using System.Collections.Generic;
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
        
        public uint Id
        {
            get => Model.Id;
            set => Model.Id = value;
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

        HashSet<uint> _linkIds = [];

        public string LinkIds => string.Join(",", _linkIds);

        public void AddLink(uint id)
        {
            _linkIds.Add(id);
            OnPropertyChanged(nameof(LinkIds));
        }

        public void SetLink(uint id)
        {
            _linkIds.Clear();
            _linkIds.Add(id);
            OnPropertyChanged(nameof(LinkIds));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
