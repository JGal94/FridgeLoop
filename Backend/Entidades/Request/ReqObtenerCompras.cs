using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqObtenerCompras
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
        public string q { get; set; }
        public System.DateTime? desde { get; set; }
        public System.DateTime? hasta { get; set; }
    }
}
