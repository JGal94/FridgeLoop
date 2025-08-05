using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Response
{
    public class ResNotificacionObtener
    {
        public string Tipo { get; set; }
        public string Mensaje { get; set; }
        public bool Leida { get; set; }
        public DateTime Fecha { get; set; }
    }
}
