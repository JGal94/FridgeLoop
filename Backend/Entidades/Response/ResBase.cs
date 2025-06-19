using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Response
{
    public class ResBase
    {
        public bool resultado { get; set; }
        public List<Error> listaDeErrores { get; set; }
    }

}
