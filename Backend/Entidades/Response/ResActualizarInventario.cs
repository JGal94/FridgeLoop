using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Enum;
using Entidades.Entity;

namespace Entidades.Response
{
    public class ResActualizarInventario : ResBase
    {
        // eco útil para el cliente
        public int idProducto { get; set; }
        public decimal? cantidad { get; set; }
        public DateTime? fechaExpiracion { get; set; }

        public string mensaje { get; set; }
    }
}
