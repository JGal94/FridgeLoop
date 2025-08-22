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
        private const string GMAIL_APP_PASSWORD = "qcee ijpf kfnj dedv"; // Cambia por tu password de aplicación

        /// <summary>
        /// Envía un correo de validación de cuenta
        /// </summary>
        /// <param name="emailUsuario">Email del usuario que se está registrando</param>
        /// <param name="codigoActivacion">Código de activación para validar la cuenta</param>
        /// <returns>True si se envió correctamente, False si hubo error</returns>
        /// 

        


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
        // Requiere: using System.Linq; using System.Net;
        // C# 7.3 compatible
        // Requiere: using System.Linq; using System.Net;
        private static string GenerarHtmlCorreo(string codigoActivacion, string verifyUrl = null, string nombre = null)
        {
            // Normaliza entradas a no nulas
            codigoActivacion = (codigoActivacion ?? string.Empty).Trim().ToUpper();
            var nombreSeguro = nombre == null ? null : WebUtility.HtmlEncode(nombre);
            var verifyUrlSeguro = verifyUrl == null ? null : WebUtility.HtmlEncode(verifyUrl);

            // Cajitas por carácter
            var codeBoxes = string.Concat(
                codigoActivacion.Select(ch =>
                    "<span style='display:inline-block;padding:12px 14px;border:1px solid #E5E7EB;border-radius:10px;margin:0 4px;background:#F8FAFC;font:600 22px ui-monospace,SFMono-Regular,Menlo,Consolas,\"Liberation Mono\",monospace;color:#0F172A;'>" +
                    WebUtility.HtmlEncode(ch.ToString()) +
                    "</span>"
                )
            );

            var saludo = string.IsNullOrWhiteSpace(nombreSeguro) ? "¡Hola!" : "¡Hola, " + nombreSeguro + "!";

            var cta = string.IsNullOrWhiteSpace(verifyUrlSeguro)
                ? ""
                : @"
            <tr>
              <td align='center' style='padding: 6px 0 0 0;'>
                <a href='" + verifyUrlSeguro + @"' 
                   style='display:inline-block;background:#2563EB;color:#FFFFFF;text-decoration:none;padding:14px 22px;border-radius:10px;font:600 15px -apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica,Arial,sans-serif;'>
                   Validar cuenta
                </a>
              </td>
            </tr>
            <tr><td style='height:16px;line-height:16px;font-size:0'>&nbsp;</td></tr>
        ";

            return @"
<!DOCTYPE html>
<html lang='es'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
  <title>Activa tu cuenta | FridgeLoop</title>
</head>
<body style='margin:0;padding:0;background:#F6F8FC;'>
  <div style='display:none;max-height:0;overflow:hidden;color:transparent;opacity:0;'>
    Tu código FridgeLoop: " + WebUtility.HtmlEncode(codigoActivacion) + @". Válido por 24 horas.
  </div>

  <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='background:#F6F8FC;'>
    <tr>
      <td align='center' style='padding:28px 16px;'>
        <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='max-width:600px;background:#FFFFFF;border:1px solid #E5E7EB;border-radius:14px;box-shadow:0 2px 10px rgba(15,23,42,0.06);'>
          <tr>
            <td align='center' style='padding:22px 22px 8px 22px;'>
              <div style='font:800 18px -apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica,Arial,sans-serif;letter-spacing:.3px;color:#2563EB;'>FridgeLoop</div>
              <div style='font:500 12px -apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#64748B;'>Gestiona tu despensa y compras</div>
            </td>
          </tr>
          <tr><td style='border-top:1px solid #EEF2F7'></td></tr>
          <tr>
            <td style='padding:24px 22px 6px 22px;font:400 15px -apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#0F172A;line-height:1.6;'>
              <div style='font-weight:700;font-size:18px;margin:0 0 6px 0;'>" + saludo + @"</div>
              <div>Gracias por registrarte en <strong>FridgeLoop</strong>. Usa el siguiente código para activar tu cuenta:</div>
            </td>
          </tr>
          <tr>
            <td align='center' style='padding:8px 22px 4px 22px;'>
              <div>" + codeBoxes + @"</div>
            </td>
          </tr>
          " + cta + @"
          <tr>
            <td style='padding:0 22px 22px 22px;font:400 13px -apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#64748B;line-height:1.6;'>
              <div><strong>Tip:</strong> Si el botón no funciona, abre la app y pega el código en la pantalla de validación.</div>
              <div style='margin-top:6px;'><strong>Seguridad:</strong> el código expira en 24 horas.</div>
            </td>
          </tr>
          <tr><td style='border-top:1px solid #EEF2F7'></td></tr>
          <tr>
            <td align='center' style='padding:16px 22px 20px 22px;font:400 12px -apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#94A3B8;'>
              Recibiste este correo porque creaste una cuenta en FridgeLoop.<br/>
              Si no fuiste tú, puedes ignorarlo sin problema.<br/><br/>
              © 2025 FridgeLoop. Todos los derechos reservados.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }


    }
}
