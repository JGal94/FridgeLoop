using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Request
{
    class ReqInsertarReceta
    {
        public Receta receta {  get; set; } 
        public List<Ingrediente> Ingredientes { get; set;}

    }
}
