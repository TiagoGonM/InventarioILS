using InventarioILS.Model.Storage;
using System;

namespace InventarioILS.Model
{
    public class Order : IIdentifiable
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public Order() { }

        public Order(uint id, string name, string description, DateTime createdAt)
        {
            Id = id;
            Name = name;
            Description = description;
            CreatedAt = createdAt;
        }
    }
}
