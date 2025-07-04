using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Response
{
    public class ResObtenerIngredientes
    {
        public int IngredientID { get; set; }
        public int RecipeID { get; set; }
        public int ProductID { get; set; }
        public decimal Quantity { get; set; }
    }
}
