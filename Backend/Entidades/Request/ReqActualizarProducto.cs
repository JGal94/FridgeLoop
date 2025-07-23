using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqActualizarProducto
    {
        public int idProducto { get; set; }
        public string nombre { get; set; }
        public int idCategoria { get; set; }
        public string unidad { get; set; }
    }
}