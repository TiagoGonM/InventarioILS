using InventarioILS.Model.Storage;

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
}
