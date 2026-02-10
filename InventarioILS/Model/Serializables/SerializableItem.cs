using CsvHelper.Configuration.Attributes;
using InventarioILS.Services;
using System.Security.Cryptography;

namespace InventarioILS.Model.Serializables
{
    public class SerializableItem
    {
        [Name(["Categoría", "categoría", "Categoria", "categoria", "Category", "category"])]
        public string Category { get; set; }

        [Name(["Subcategoría", "subcategoría", "Subcategoria", "subcategoria", "Subcategory", "subcategory"])]
        public string Subcategory { get; set; }

        [Name(["Clase", "clase", "Tipo", "tipo", "Class", "class"])]
        public string Class { get; set; }

        [Name(["Estado", "estado", "State", "state"])]
        public string State { get; set; }

        [Name([
            "Modelo o valor", "modelo o valor",
            "Model or value", "model or value",
            "Modelo/Valor", "Modelo/valor", "modelo/valor",
            "Model/Value", "Model/value", "model/value",
            "Models/Values", "Model/values", "models/values",
            "Valores/Modelos", "Valores/modelos", "valores/modelos", "Modelos/Valores", "Modelos/valores", "modelos/valores",
            "Modelo", "modelo", "Valor", "valor"])]
        public string ModelOrValue { get; set; }

        [Name([
            "Extra", "extra",
            "Notas adicionales", "notas adicionales", "NotasAdicionales", "notasadicionales",
            "Additional notes", "additional notes", "AdditionalNotes", "additionalnotes"])]
        public string AdditionalNotes { get; set; }

        [Name(["Total", "total", "Cantidad", "cantidad", "Quantity", "quantity"])]
        public uint Quantity { get; set; }

        [Name([
            "Ubicación", "ubicación", "Ubicacion", "ubicacion", 
            "Locación", "locación", "Locacion", "locacion",
            "Localización", "localización", "Localizacion", "localizacion",
            "Location", "location"])]
        public string Location { get; set; }
    }
}
