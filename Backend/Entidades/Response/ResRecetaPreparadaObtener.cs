using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Response
{
    public class ResRecetaPreparadaObtener
    {
        public int Id { get; set; }
        public string NombreReceta { get; set; }
        public DateTime FechaPreparacion { get; set; }
    }
}
