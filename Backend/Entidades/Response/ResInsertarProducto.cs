using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Response
{
    public class ResInsertarProducto : ResBase
    {
        public int idProducto { get; set; }
        public string mensaje { get; set; }
    }
}