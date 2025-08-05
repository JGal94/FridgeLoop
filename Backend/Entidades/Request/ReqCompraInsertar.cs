using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqCompraInsertar
    {
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public decimal MontoTotal { get; set; }
    }

}
