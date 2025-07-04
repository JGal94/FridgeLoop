using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
   public class ReqInsertarReseta
    {

        public class ReqInsertarReceta
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int PreparationTime { get; set; }
            public string Difficulty { get; set; }
            public int Calories { get; set; }
            public string Style { get; set; }
            public int UserID { get; set; }

            public List<IngredienteReceta> Ingredients { get; set; }
        }

        public class IngredienteReceta
        {
            public int ProductID { get; set; }
            public decimal Quantity { get; set; }
        }

    }
}
