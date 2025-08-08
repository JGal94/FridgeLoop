using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend_Proyecto_Fridgeloop.Entidades.Entity
{
    public class Sesion
    {
        public int id { get; set; }

        public Usuario usuario { get; set; }

        public string token { get; set; }

        public string origen { get; set; } // Dispositivo o app

        public string direccionIP { get; set; }

        public DateTime fechaExpiracion { get; set; }

        public DateTime fechaCreacion { get; set; } // opcional, si lo quieres rastrear
    }
}
