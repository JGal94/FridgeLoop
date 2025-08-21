using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Enum
{
    public enum EnumErrores
    {
        ErrorNoControlado = 100,
        ErrorDeBaseDatos = 101,
        ConexionFallida = 102,

        // ============================
        // 200–299 → Errores de validación / entrada de datos
        // ============================

        Validacion = 200,                     // Error genérico de validación
        RequestNulo = 201,
        NombreNuloOVacio = 202,
        ApellidoNuloOVacio = 203,
        CorreoNuloOVacio = 204,
        FormatoCorreoInvalido = 205,
        PasswordNuloOVacio = 206,
        PasswordDebil = 207,
        UsuarioFaltante = 208,
        CampoRequeridoFaltante = 209,
        FormatoDatoInvalido = 210,            // Algún dato enviado no cumple el formato esperado

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
