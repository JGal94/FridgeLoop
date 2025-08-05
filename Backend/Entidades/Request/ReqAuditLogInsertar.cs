using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqAuditLogInsertar
    {
        public string Tabla { get; set; }
        public int IdRegistro { get; set; }
        public string Accion { get; set; }
        public int IdUsuario { get; set; }
        public string DatosAnteriores { get; set; }
        public string DatosNuevos { get; set; }
    }

}
