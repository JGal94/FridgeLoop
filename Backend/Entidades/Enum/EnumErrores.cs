using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Enum
{
    public enum EnumErrores
    {
        errorNoControlado = -2,
        errorDeBaseDatos = -1,
        requestNulo = 1,
        nombreNuloVacio = 2,
        apellidoNuloVacio = 3,
        correoNuloVacio = 4,
        formatoCorreoIncorrecto = 5,
        passwordNuloVacio = 6,
        passwordDebil = 7,
        usuarioFaltante = 8,
        usuarioNoEncontrado = 9,
        noSeEnvioElCorreo = 10,
        credencialesIncorrectas = 11,
        usuarioNoActivado = 12,
        tokenExpirado = 13,
    }
}
//pedirle a una ia que arregle esto