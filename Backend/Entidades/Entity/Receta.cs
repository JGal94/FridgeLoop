using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Entity
{
   public class Receta
    {
        public int RecipeID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PreparationTime { get; set; }
        public string Difficulty { get; set; }
        public int Calories { get; set; }
        public string Style { get; set; }
    }
}
