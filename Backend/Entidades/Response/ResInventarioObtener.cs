using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Response
{
    public class ResInventarioObtener
    {
        public int Id { get; set; }
        public string Producto { get; set; }
        public decimal Cantidad { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Categoria { get; set; }
    }
}
