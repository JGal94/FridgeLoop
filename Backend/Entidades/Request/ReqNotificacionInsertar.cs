using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqNotificacionInsertar
    {
        public int IdUsuario { get; set; }
        public string Mensaje { get; set; }
        public string Tipo { get; set; }
    }

}
