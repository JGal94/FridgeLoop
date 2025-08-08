using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend_Proyecto_Fridgeloop.Entidades.Entity
{
    public class InventarioUsuario
    {
        public int InventoryID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public decimal Quantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
