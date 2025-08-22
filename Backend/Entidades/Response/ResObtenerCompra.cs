using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Response
{
    // Entidades/Response/ResObtenerCompra.cs
    using System;
    using System.Collections.Generic;

    namespace Entidades.Response
    {
        public class ResObtenerCompra
        {
            public bool resultado { get; set; }
            public string mensaje { get; set; }
            public List<Error> listaDeErrores { get; set; } = new List<Error>();
            public CompraDetalle compra { get; set; }                 // ← cambia el tipo
        }

        public class CompraDetalle
        {
            public int idCompra { get; set; }
            public DateTime fechaCompra { get; set; }
            public decimal total { get; set; }
            public string notas { get; set; }
            public List<ItemCompra> items { get; set; } = new List<ItemCompra>();
        }

        public class ItemCompra
        {
            public int idProducto { get; set; }
            public string nombre { get; set; }
            public int idCategoria { get; set; }
            public string unidad { get; set; }
            public decimal cantidad { get; set; }
            public decimal? precioUnitario { get; set; }
        }
    }

}
