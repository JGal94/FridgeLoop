using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Response
{
    public class ResEliminarCompra
    {
        public bool resultado { get; set; }
        public string mensaje { get; set; }
        public System.Collections.Generic.List<Error> listaDeErrores { get; set; }
    }
}
