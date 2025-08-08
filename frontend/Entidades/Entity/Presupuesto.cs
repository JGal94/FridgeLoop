using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend_Proyecto_Fridgeloop.Entidades.Entity
{
    public class Presupuesto
    {
        public int BudgetID { get; set; }
        public int UserID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal MaxAmount { get; set; }
    }
}
