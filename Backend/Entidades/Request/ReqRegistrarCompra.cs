using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqRegistrarCompra
    {
        public System.DateTime? fechaCompra { get; set; }
        public string notas { get; set; }
        public System.Collections.Generic.List<CompraItemReq> items { get; set; }
    }

    public class CompraItemReq
    {
        public int idProducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal? precioUnitario { get; set; }
        public System.DateTime? fechaExpiracion { get; set; }
    }
}
