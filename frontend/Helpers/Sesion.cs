using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend_Proyecto_Fridgeloop.Helpers
{
    public static class Sesion
    {
        public static int Id { get; set; }
        public static string Token { get; set; } = "";
        public static string Nombre { get; set; } = "";
        public static string Correo { get; set; } = "";
    }
}
