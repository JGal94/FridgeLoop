using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Request
{
    public class ReqProductosPorVencer
    {
        public int dias { get; set; } = 7;          // ventana: próximos N días
        public bool incluirVencidos { get; set; } = false; // incluye también expirados
    }
}
