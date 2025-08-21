using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Frontend_Proyecto_Fridgeloop.Helpers;

namespace Frontend_Proyecto_Fridgeloop.Services
{
    // Usa el mismo HttpServiceBase que ya tienes (Http, J, SendAsync<T>)
    public class CompraService : HttpServiceBase
    {
        public class ResBase
        {
            public bool resultado { get; set; }
            public string? mensaje { get; set; }
            public List<ErrorDto>? listaDeErrores { get; set; }
        }

        public class ErrorDto { public int ErrorCode { get; set; } public string Message { get; set; } = ""; }

        public class ResRegistrarCompra : ResBase
        {
            public int idCompra { get; set; }
            public decimal total { get; set; }
        }

        // Contrato backend: ReqRegistrarCompra + ItemCompraSinId
        // (fechaCompra?, items: [{nombre, idCategoria, unidad, cantidad, precioUnitario?, fechaExpiracion?}])
        // ref: POST /api/compra/registrar  
        public async Task<ResRegistrarCompra?> RegistrarCompraAsync(DateTime? fecha, IEnumerable<ShoppingItem> items, CancellationToken ct = default)
        {
            var body = new
            {
                fechaCompra = fecha,
                items = items.Select(i => new
                {
                    nombre = i.Nombre,
                    idCategoria = i.IdCategoria,
                    unidad = i.Unidad,
                    cantidad = i.Cantidad,
                    precioUnitario = i.PrecioUnitario,
                    fechaExpiracion = i.FechaExpiracion
                }).ToList()
            };

            return await SendAsync<ResRegistrarCompra>(() => Http.PostAsync("api/compra/registrar", J(body), ct), ct);
        }

        public static string FirstError(ResBase? r, string fallback = "Ocurrió un error.")
            => r?.listaDeErrores?.FirstOrDefault()?.Message ?? r?.mensaje ?? fallback;
    }
}