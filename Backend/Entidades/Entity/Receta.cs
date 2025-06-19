using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Entity
{
    public class Receta
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int TiempoPreparacion { get; set; }
        public string Dificultad { get; set; }
        public int Calorias { get; set; }
        public string Estilo { get; set; }

        // Si estás usando ingredientes no ligados a catálogo
        public List<Ingrediente> Ingredientes { get; set; }
    }
}
