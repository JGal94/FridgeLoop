using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Backend; // Para LogicaSesion

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class JwtAuthorizeAttribute : AuthorizationFilterAttribute
{
    // Estos valores deben coincidir con los usados al generar el token
    private const string Issuer = "tuApp";
    private const string Audience = "tusUsuarios";
    private const string Secret = "Felipensativo"; // Mueve esto al web.config en producción

    public override void OnAuthorization(HttpActionContext actionContext)
    {
        try
        {
            // 1. Obtener el token del header Authorization: Bearer {token}
            var authHeader = actionContext.Request.Headers.Authorization;
            if (authHeader == null || !string.Equals(authHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
            {
                Denegar(actionContext, "Encabezado Authorization no presente o mal formado.");
                return;
            }

            var token = authHeader.Parameter;
            if (string.IsNullOrWhiteSpace(token))
            {
                Denegar(actionContext, "Token vacío.");
                return;
            }

            // 2. Validar el JWT (firma, expiración, issuer, audience)
            var tokenHandler = new JwtSecurityTokenHandler();
            var clave = Encoding.UTF8.GetBytes(Secret);
            var parametros = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(clave),
                ClockSkew = TimeSpan.Zero // Sin tolerancia de tiempo
            };

            SecurityToken tokenValidado;
            var principal = tokenHandler.ValidateToken(token, parametros, out tokenValidado);

            // 3. Verificar si la sesión está activa en la base de datos
            var logSesion = new LogicaSesion();
            if (!logSesion.EsSesionActiva(token))
            {
                Denegar(actionContext, "Sesión no activa o ya cerrada.");
                return;
            }

            // 4. Asignar usuario autenticado al contexto actual
            Thread.CurrentPrincipal = principal;
            if (actionContext.RequestContext != null)
            {
                actionContext.RequestContext.Principal = principal;
            }
        }
        catch (SecurityTokenException ex)
        {
            Denegar(actionContext, "Token inválido o expirado: " + ex.Message);
        }
        catch (Exception ex)
        {
            Denegar(actionContext, "Error al validar token: " + ex.Message);
        }
    }

    private void Denegar(HttpActionContext ctx, string mensaje)
    {
        ctx.Response = ctx.Request.CreateResponse(HttpStatusCode.Unauthorized, new
        {
            resultado = false,
            mensaje = mensaje
        });
    }
}
