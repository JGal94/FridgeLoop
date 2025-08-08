using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend_Proyecto_Fridgeloop.Entidades.Entity
{
    public class Ingrediente
    {
        public int IngredientID { get; set; }     // Opcional si solo es lectura
        public int RecipeID { get; set; }
        public int ProductID { get; set; }
        public decimal Quantity { get; set; }

    }
}
