using Entidades.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Response
{
    public class ResListaCompra
    {
        public bool resultado { get; set; }
        public string mensaje { get; set; }
        public List<ProductoRecomendado> productos { get; set; }
        public List<Error> listaDeErrores { get; set; }
    }
}
