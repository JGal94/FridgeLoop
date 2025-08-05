using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Response
{
    public class ResAuditLogObtener
    {
        public string Tabla { get; set; }
        public string Accion { get; set; }
        public string ModificadoPor { get; set; }
        public DateTime Fecha { get; set; }
    }
}
