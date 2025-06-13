using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Entity
{
    public class Usuario
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string correoElectronico { get; set; }
        public string password { get; set; }
    }
}
