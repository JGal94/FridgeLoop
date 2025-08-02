using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Response
{
    public class ResPresupuestoObtener
    {
        public int Mes { get; set; }
        public int Año { get; set; }
        public decimal MontoMaximo { get; set; }
        public decimal Gastado { get; set; }
        public bool Superado => Gastado > MontoMaximo;
    }
}
