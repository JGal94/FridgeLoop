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
using System.Configuration;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class JwtAuthorizeAttribute : AuthorizationFilterAttribute
{

    

    public override void OnAuthorization(HttpActionContext actionContext)
    {

        try
        {
            // 1) Tomar el header Authorization: Bearer {token}
            var authHeader = actionContext.Request.Headers.Authorization;
            if (authHeader == null || !string.Equals(authHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(authHeader.Parameter))
            {
                Deny(actionContext, "Falta el token Bearer.");
                return;
            }

            var token = authHeader.Parameter;

            // 2) Leer configuración desde web.config (solo en la API)
            var issuer = ConfigurationManager.AppSettings["Jwt:Issuer"];
            var audience = ConfigurationManager.AppSettings["Jwt:Audience"];
            var secret = ConfigurationManager.AppSettings["Jwt:Secret"];

            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(secret))
            {
                Deny(actionContext, "Configuración JWT incompleta en web.config.");
                return;
            }

            // 3) Validar firma/claims/expiración del JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            ClaimsPrincipal principal;
            SecurityToken validatedToken;
            try
            {
                principal = tokenHandler.ValidateToken(token, parameters, out validatedToken);
            }
            catch (Exception)
            {
                Deny(actionContext, "Token inválido o expirado.");
                return;
            }

            // 4) Verificar sesión activa en BD
            var sesionActiva = new LogicaSesion().EsSesionActiva(token);
            if (!sesionActiva)
            {
                Deny(actionContext, "Sesión no activa o expirada.");
                return;
            }

            // 5) Setear principal para el pipeline de Web API
            Thread.CurrentPrincipal = principal;
            if (actionContext.ControllerContext != null)
            {
                // Si usas ApiController.User, esto lo hace visible
                actionContext.ControllerContext.RequestContext.Principal = principal;
            }
        }
        catch (Exception)
        {
            Deny(actionContext, "Error interno al validar autorización.");
        }
    }

    private void Deny(HttpActionContext ctx, string message)
    {
        ctx.Response = ctx.Request.CreateResponse(HttpStatusCode.Unauthorized, new
        {
            ok = false,
            error = message
        });
    }
}