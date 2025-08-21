using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Response
{
    public class ResRegistrarCompra : ResBase
    {
        public int idCompra { get; set; }
        public decimal total { get; set; }
        public string mensaje { get; set; }   // ✅ ahora sí compila
    }
}
