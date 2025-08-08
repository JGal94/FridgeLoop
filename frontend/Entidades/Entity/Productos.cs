using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend_Proyecto_Fridgeloop.Entidades.Entity
{
    public class Productos
    {
        public string nombre { get; set; }
        public int idCategoria { get; set; }
        public string unidad { get; set; }
        public int? userID { get; set; }
        public decimal? quantity { get; set; }
        public DateTime? expirationDate { get; set; }
    }
}
