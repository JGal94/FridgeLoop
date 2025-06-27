using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqLogin
    {
        public string correo {  get; set; }
        public string password { get; set; }
        public string origen { get; set; }
    }
}
