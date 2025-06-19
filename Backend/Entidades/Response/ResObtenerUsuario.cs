using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Response
{
    public class ResObtenerUsuario : ResBase
    {
        public Usuario usuario { get; set; }
    }
}
