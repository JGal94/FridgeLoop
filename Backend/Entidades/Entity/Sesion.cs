using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Entity
{
    public class Sesion
    {
        public int Id { get; set; }
        public Usuario Usuario { get; set; }
        public string Token { get; set; }
        public string Origen { get; set; }
    }
}
