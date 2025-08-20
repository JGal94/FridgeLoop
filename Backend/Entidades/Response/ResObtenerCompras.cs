using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Response
{
    public class ResObtenerCompras
    {
        public bool resultado { get; set; }
        public string mensaje { get; set; }
        public System.Collections.Generic.List<Error> listaDeErrores { get; set; }
        public System.Collections.Generic.List<CompraResumen> compras { get; set; }
        public int totalFiltrado { get; set; }
    }

    public class CompraResumen
    {
        public int idCompra { get; set; }
        public DateTime fechaCompra { get; set; }
        public decimal total { get; set; }
        public decimal items { get; set; }   // <-- CAMBIAR a decimal
        public string notas { get; set; }
    }

}
