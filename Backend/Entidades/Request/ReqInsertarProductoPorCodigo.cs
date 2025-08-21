using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqInsertarProductoPorCodigo
    {
        public string codigoBarras { get; set; }
        public DateTime? fechaExpiracion { get; set; }
        public decimal cantidad { get; set; }
    }
}
