using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Entity
{
    public class Sesion
    {
        public int id;
        public Usuario usuario;
        public string token;
        public string origen;
    }
}
