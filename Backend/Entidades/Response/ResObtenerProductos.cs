using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Response
{
    // Heredamos de ResBase para mantener estructura estándar
    public class ResObtenerProductos : ResBase
    {
        // Lista de productos que se devolverá en la respuesta
        public List<Productos> productos { get; set; }
    }
}