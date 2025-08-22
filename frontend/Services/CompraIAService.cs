using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frontend_Proyecto_Fridgeloop.Helpers;

namespace Frontend_Proyecto_Fridgeloop.Services
{
    // Usa tu HttpServiceBase (BaseApi, bearer, SendAsync<T>, etc.)
    public class CompraIAService : HttpServiceBase
    {
        public class ErrorDto { public int ErrorCode { get; set; } public string Message { get; set; } = ""; }
        public class ResBase { public bool resultado { get; set; } public string? mensaje { get; set; } public List<ErrorDto>? listaDeErrores { get; set; } }

        public class ProductoRecomendadoDto
        {
            public string? nombre { get; set; }
            public string? unidad { get; set; }
            public decimal cantidadRecomendada { get; set; }
        }

        public class ResListaCompra : ResBase
        {
            public List<ProductoRecomendadoDto>? productos { get; set; }
        }

        public async Task<ResListaCompra?> ObtenerPrediccionAsync(CancellationToken ct = default)
        {
            var url = "api/compra/predice-lista-compra";
            return await SendAsync<ResListaCompra>(() => Http.GetAsync(url, ct), ct);
        }

        public static string FirstError(ResBase? r, string fallback = "Ocurrió un error.")
            => r?.listaDeErrores?.FirstOrDefault()?.Message ?? r?.mensaje ?? fallback;
    }
}
