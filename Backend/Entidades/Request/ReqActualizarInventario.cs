using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqActualizarInventario
    {
        public int idProducto { get; set; }
        public decimal? cantidad { get; set; }          // nueva cantidad (si aplicara)
        public DateTime? fechaExpiracion { get; set; }  // preferible en UTC (ISO 8601)
    }
}
