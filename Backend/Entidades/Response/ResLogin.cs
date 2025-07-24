using Entidades.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Response
{
    public class ResLogin : ResBase
    {
        public Usuario Usuario { get; set; }
        

        // 🔐 Nuevo campo para el token JWT
        public string TokenJwt { get; set; }
    }
}
