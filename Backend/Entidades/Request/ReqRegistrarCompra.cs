using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    
        public class ReqRegistrarCompra
        {
            public DateTime? fechaCompra { get; set; }

            // ✅ C# 7.3: especifica el tipo
            public List<ItemCompraSinId> items { get; set; } = new List<ItemCompraSinId>();

            // O si prefieres, con constructor:
            // public ReqRegistrarCompra() { items = new List<ItemCompraSinId>(); }
        }

        public class ItemCompraSinId
        {
            public string nombre { get; set; }          // requerido
            public int idCategoria { get; set; }        // requerido
            public string unidad { get; set; }          // requerido
            public decimal cantidad { get; set; }       // > 0
            public decimal? precioUnitario { get; set; } // default 0 si no viene
            public DateTime? fechaExpiracion { get; set; } // opcional
        }
    }




