using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Response
{
    public class ResRegistrarCompra
    {
        public bool resultado { get; set; }
        public string mensaje { get; set; }
        public List<Error> listaDeErrores { get; set; }

        public int idCompra { get; set; }  // 0 si aún no persistes compras en BD
        public decimal total { get; set; }
    }
}
