using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqEliminarCompra
    {
        public int idCompra { get; set; }
        public bool revertirInventario { get; set; }
    }
}
