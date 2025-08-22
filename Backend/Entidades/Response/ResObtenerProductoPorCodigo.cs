using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Entity;

namespace Entidades.Response
{
    public class ResObtenerProductoPorCodigo : ResBase
    {
        public Productos Producto { get; set; }

    }
}
