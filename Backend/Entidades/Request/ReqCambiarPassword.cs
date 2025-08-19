using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqCambiarPassword
    {
        public string passwordActual { get; set; }
        public string passwordNueva { get; set; }
        public string confirmarPassword { get; set; }
    }
}
