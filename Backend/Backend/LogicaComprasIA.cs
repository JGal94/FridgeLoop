using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    using Entidades.Entity;
    using Entidades.Enum;
    using Entidades.Response;
    using Gateway;
    using Gateway.Request;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    namespace Backend
    {
        public class LogicaComprasIA
        {
            /// <summary>
            /// Genera la lista de compra solo usando DeepSeek,
            /// en base al inventario actual del usuario.
            /// </summary>
            /// 

            public LogicaComprasIA()
            {
            }
            public async Task<ResListaCompra> GenerarListaCompraConIA(int userId, List<Productos> inventario)
            {
                var res = new ResListaCompra
                {
                    productos = new List<ProductoRecomendado>(),
                    listaDeErrores = new List<Error>()
                };

                if (userId <= 0)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.TokenInvalido,
                        Message = "Usuario no autenticado."
                    });
                    return res;
                }

                if (inventario == null) inventario = new List<Productos>();

                // --- 1) Preparar prompt ---
                var invCompacto = inventario.Select(p => new
                {
                    nombre = p.nombre,
                    unidad = p.unidad,
                    cantidad = p.quantity,
                    expira = p.expirationDate?.ToString("yyyy-MM-dd")
                }).ToList();

                string inventarioJson = JsonConvert.SerializeObject(invCompacto);

                string prompt =
                    "Eres un asistente que genera listas de compra optimizadas para evitar desperdicio y sobrestock.\n" +
                    "Reglas:\n" +
                    "1) Prioriza reponer lo justo para el ciclo de compra promedio (7-14-30 días).\n" +
                    "2) Evita recomendar cantidades que excedan lo que se consume típicamente.\n" +
                    "3) No recomiendes productos que ya están en suficiente cantidad salvo que vayan a vencer antes del próximo ciclo.\n" +
                    "4) Devuelve SOLO JSON puro en este formato:\n" +
                    "[{ \"nombre\": \"string\", \"unidad\": \"string\", \"cantidadRecomendada\": decimal }]\n\n" +
                    $"InventarioActual = {inventarioJson}\n\n" +
                    "Genera la lista ajustada según este inventario.";

                // --- 2) Construir request ---
                var request = new DeepSeekRequest
                {
                    Model = "deepseek-chat",
                    Messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "user", Content = prompt }
                }
                };

                // --- 3) Llamar a DeepSeek ---
                try
                {
                    using (var deepseek = new DeepSeekApiClient())
                    {
                        var respuesta = await deepseek.SendChatRequestAsync(request);
                        var raw = respuesta?.Choices?.FirstOrDefault()?.Message?.Content ?? "";

                        // Por si devuelve con ```json ...
                        raw = raw.Trim();
                        if (raw.StartsWith("```"))
                            raw = raw.Replace("```json", "").Replace("```", "").Trim();

                        var sugeridos = JsonConvert.DeserializeObject<List<ProductoRecomendado>>(raw) ?? new List<ProductoRecomendado>();

                        // Normalizar
                        foreach (var pr in sugeridos)
                        {
                            pr.nombre = (pr.nombre ?? "").Trim();
                            pr.unidad = string.IsNullOrWhiteSpace(pr.unidad) ? "unidad" : pr.unidad.Trim();
                            if (pr.cantidadRecomendada < 0) pr.cantidadRecomendada = 0;
                        }

                        res.productos = sugeridos;
                        res.resultado = true;
                        res.mensaje = "Lista de compra generada con IA.";
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    res.resultado = false;
                    res.listaDeErrores.Add(new Error
                    {
                        ErrorCode = EnumErrores.IARecomendacionFallida,
                        Message = "DeepSeek no pudo generar la lista: " + ex.Message
                    });
                    return res;
                }
            }
        }
    }

}
