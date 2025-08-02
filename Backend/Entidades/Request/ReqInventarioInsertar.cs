using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqInventarioInsertar
    {
        public int IdUsuario { get; set; }
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }

}
