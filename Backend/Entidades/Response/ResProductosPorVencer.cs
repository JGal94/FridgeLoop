using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Response
{
    public class ResProductosPorVencer
    {
        public bool resultado { get; set; }
        public string mensaje { get; set; }
        public List<Error> listaDeErrores { get; set; }
        public List<ProductoPorVencer> productos { get; set; }
    }

    public class ProductoPorVencer
    {
        public int idProducto { get; set; }
        public string nombre { get; set; }
        public int idCategoria { get; set; }
        public string unidad { get; set; }
        public decimal? cantidad { get; set; }
        public System.DateTime? fechaExpiracion { get; set; }
        public int? diasRestantes { get; set; }   // <-- nullable
    }
}
