using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class Utilitarios
    {
        #region
        public  const string passwordAplicacion = "SuPasswordUltraSecreto =P";
        #endregion

        // Configuración de la cuenta de Gmail
        private const string GMAIL_SMTP = "smtp.gmail.com";
        private const int GMAIL_PORT = 587;
        private const string GMAIL_EMAIL = "tudespensaia@gmail.com"; // Cambia por tu email
        private const string GMAIL_APP_PASSWORD = "2025despensa++**2025"; // Cambia por tu password de aplicación

        /// <summary>
        /// Envía un correo de validación de cuenta
        /// </summary>
        /// <param name="emailUsuario">Email del usuario que se está registrando</param>
        /// <param name="codigoActivacion">Código de activación para validar la cuenta</param>
        /// <returns>True si se envió correctamente, False si hubo error</returns>
        public static bool EnviarCorreoValidacion(string emailUsuario, string codigoActivacion)
        {
            try
            {
                // Configurar el cliente SMTP
                using (SmtpClient smtpClient = new SmtpClient(GMAIL_SMTP, GMAIL_PORT))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(GMAIL_EMAIL, GMAIL_APP_PASSWORD);

                    // Crear el mensaje
                    using (MailMessage mensaje = new MailMessage())
                    {
                        mensaje.From = new MailAddress(GMAIL_EMAIL, "Foro App");
                        mensaje.To.Add(emailUsuario);
                        mensaje.Subject = "Validación de Cuenta - Código de Activación";
                        mensaje.Body = GenerarHtmlCorreo(codigoActivacion);
                        mensaje.IsBodyHtml = true;

                        // Enviar el correo
                        smtpClient.Send(mensaje);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Opcionalmente puedes loggear el error
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Genera el HTML del correo de validación
        /// </summary>
        /// <param name="codigoActivacion">Código de activación</param>
        /// <returns>HTML del correo</returns>
        private static string GenerarHtmlCorreo(string codigoActivacion)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Validación de Cuenta</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
        
        <!-- Header -->
        <div style='text-align: center; margin-bottom: 30px;'>
            <h1 style='color: #333; margin: 0;'>¡Bienvenido!</h1>
            <p style='color: #666; margin: 10px 0 0 0;'>Validación de Cuenta</p>
        </div>

        <!-- Contenido principal -->
        <div style='margin-bottom: 30px;'>
            <p style='color: #333; font-size: 16px; line-height: 1.5;'>
                Gracias por registrarte en nuestra aplicación. Para completar tu registro, 
                necesitamos validar tu dirección de correo electrónico.
            </p>
            
            <p style='color: #333; font-size: 16px; line-height: 1.5;'>
                Tu código de activación es:
            </p>
        </div>

        <!-- Código de activación -->
        <div style='text-align: center; margin: 30px 0;'>
            <div style='background-color: #f8f9fa; border: 2px dashed #007bff; padding: 20px; border-radius: 8px; display: inline-block;'>
                <span style='font-size: 32px; font-weight: bold; color: #007bff; letter-spacing: 5px;'>
                    {codigoActivacion}
                </span>
            </div>
        </div>

        <!-- Instrucciones -->
        <div style='margin-bottom: 30px;'>
            <p style='color: #333; font-size: 16px; line-height: 1.5;'>
                Ingresa este código en la página de validación para activar tu cuenta.
            </p>
            
            <p style='color: #666; font-size: 14px; line-height: 1.5;'>
                <strong>Nota:</strong> Este código expira en 24 horas por seguridad.
            </p>
        </div>

        <!-- Footer -->
        <div style='border-top: 1px solid #eee; padding-top: 20px; text-align: center;'>
            <p style='color: #999; font-size: 12px; margin: 0;'>
                Si no solicitaste este registro, puedes ignorar este correo.
            </p>
            <p style='color: #999; font-size: 12px; margin: 10px 0 0 0;'>
                © 2025 Tu Aplicación. Todos los derechos reservados.
            </p>
        </div>

    </div>
</body>
</html>";
        }
    }
}
