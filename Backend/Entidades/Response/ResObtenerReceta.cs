using Entidades.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Response
{
   public class ResObtenerReceta
    {
        public int RecipeID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PreparationTime { get; set; }
        public string Difficulty { get; set; }
        public int Calories { get; set; }
        public string Style { get; set; }
        public DateTime PreparedAt { get; set; }

       // public List<Ingrediente> Ingredientes { get; set; } 
    }
}
