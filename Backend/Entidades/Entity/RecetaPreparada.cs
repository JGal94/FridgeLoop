using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Entity
{
   public class RecetaPreparada
    {
        public int PreparedID { get; set; }
        public int UserID { get; set; }
        public int RecipeID { get; set; }
        public DateTime PreparedAt { get; set; }
    }
}
