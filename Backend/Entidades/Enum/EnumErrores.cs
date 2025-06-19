using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Enum
{
    public enum EnumErrores
    {
        ErrorNoControlado = 100,              // Excepción no manejada por el sistema
        ErrorDeBaseDatos = 101,               // Error al acceder o consultar la base de datos
        ConexionFallida = 102,                // No se pudo conectar al servidor o base de datos

        // ============================
        // 200–299 → Errores de validación / entrada de datos
        // ============================

        RequestNulo = 200,                    // La solicitud es nula
        NombreNuloOVacio = 201,               // El nombre está vacío o no fue enviado
        ApellidoNuloOVacio = 202,             // El apellido está vacío o no fue enviado
        CorreoNuloOVacio = 203,               // El correo no fue proporcionado
        FormatoCorreoInvalido = 204,          // El formato del correo electrónico no es válido
        PasswordNuloOVacio = 205,             // La contraseña está vacía
        PasswordDebil = 206,                  // La contraseña no cumple con requisitos mínimos
        UsuarioFaltante = 207,                // Faltan datos del usuario en la solicitud
        CampoRequeridoFaltante = 208,         // Uno o más campos obligatorios no fueron enviados
        FormatoDatoInvalido = 209,            // Algún dato enviado no cumple el formato esperado

        // ============================
        // 300–399 → Errores de autenticación / seguridad / sesión
        // ============================

        UsuarioNoEncontrado = 300,            // No se encontró un usuario con esos datos
        CredencialesIncorrectas = 301,        // Contraseña o correo electrónico incorrectos
        UsuarioNoActivado = 302,              // El usuario aún no ha activado su cuenta
        NoSeEnvioElCorreo = 303,              // Falló el envío del correo electrónico
        TokenExpirado = 304,                  // El token JWT ha expirado
        TokenInvalido = 305,                  // El token JWT es inválido o fue manipulado
        SesionNoActiva = 306,                 // La sesión no está activa o fue cerrada
        SesionExpirada = 307,                 // La sesión ha caducado
        SesionDuplicada = 308,                // Ya existe una sesión activa

        // ============================
        // 400–499 → Errores de negocio (recetas, inventario, etc.)
        // ============================

        ProductoNoEncontrado = 400,           // El producto solicitado no existe
        CategoriaNoEncontrada = 401,          // La categoría indicada no existe
        InventarioVacio = 402,                // El usuario no tiene productos en inventario
        RecetaNoEncontrada = 403,             // No se encontró la receta solicitada
        IngredientesInsuficientes = 404,      // El usuario no tiene los ingredientes necesarios
        CompraNoRegistrada = 405,             // Ocurrió un error al registrar la compra
        ErrorEnCalculoPresupuesto = 406,      // Fallo al calcular presupuesto mensual
        ProductoDuplicado = 407,              // Se intentó insertar un producto ya existente

        // ============================
        // 500–599 → Otros errores (notificaciones, AI, OCR, etc.)
        // ============================

        NotificacionNoEnviada = 500,          // Error al enviar una notificación
        OCRFallido = 501,                     // Fallo al interpretar la imagen o factura
        LecturaQRFallida = 502,               // No se pudo leer el código QR o código de barras
        IARecomendacionFallida = 503
    }
}
//pedirle a una ia que arregle esto