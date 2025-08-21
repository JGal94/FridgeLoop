using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Collections.ObjectModel;

namespace Frontend_Proyecto_Fridgeloop.Helpers
{
    // Ítem que el usuario agrega para comprar (sin id de producto)
    public class ShoppingItem
    {
        public string Nombre { get; set; } = "";
        public int IdCategoria { get; set; }
        public string Unidad { get; set; } = "";
        public decimal Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; }  // opcional
        public DateTime? FechaExpiracion { get; set; } // opcional

        // Para mostrar en la lista “quemada” actual
        public override string ToString()
        {
            var qty = Cantidad % 1 == 0 ? $"{(int)Cantidad}" : $"{Cantidad}";
            var pu = PrecioUnitario.HasValue ? $" @ {PrecioUnitario.Value:0.##}" : "";
            return $"• {Nombre} ({qty} {Unidad}){pu}";
        }
    }

    public static class ShoppingList
    {
        // Lista en memoria para la sesión actual
        public static ObservableCollection<ShoppingItem> Items { get; } = new();

        public static void Clear() => Items.Clear();

        // Para la UI actual (un Editor/Label dentro del Frame)
        public static string AsBulletedText()
        {
            if (Items.Count == 0) return "— Sin productos —";
            return string.Join(Environment.NewLine, Items.Select(i => i.ToString()));
        }
    }
}