using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqPresupuestoInsertar
    {
        public int IdUsuario { get; set; }
        public int Mes { get; set; }
        public int Año { get; set; }
        public decimal MontoMaximo { get; set; }
    }

}
