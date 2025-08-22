using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Frontend_Proyecto_Fridgeloop.Helpers
{
    public static class CategoryMapper
    {
        private static readonly Dictionary<string, int> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            // === Catálogo exacto 1..46 ===
            ["cereales y granos"] = 1,
            ["pastas y fideos"] = 2,
            ["arroz"] = 3,
            ["legumbres y frijoles"] = 4,
            ["pan y tortillas"] = 5,
            ["reposteria y galletas"] = 6,
            ["aceites y grasas"] = 7,
            ["azucar y endulzantes"] = 8,
            ["sal y especias"] = 9,
            ["salsas y condimentos"] = 10,
            ["sopas y caldos"] = 11,
            ["enlatados de verduras"] = 12,
            ["enlatados de frutas"] = 13,
            ["enlatados de pescado"] = 14,
            ["carnes rojas"] = 15,
            ["pollo y aves"] = 16,
            ["pescados y mariscos"] = 17,
            ["embutidos y fiambres"] = 18,
            ["lacteos"] = 19,
            ["huevos"] = 20,
            ["quesos"] = 21,
            ["yogures"] = 22,
            ["bebidas alcoholicas"] = 23,
            ["jugos y nectar es"] = 24,   // por si llega con espacio raro
            ["jugos y néctares"] = 24,
            ["jugos y nectares"] = 24,
            ["aguas"] = 25,
            ["bebidas energeticas"] = 26,
            ["cafe"] = 27,
            ["te e infusiones"] = 28,
            ["té e infusiones"] = 28,
            ["chocolate en polvo"] = 29,
            ["frutas frescas"] = 30,
            ["verduras frescas"] = 31,
            ["congelados"] = 32,
            ["snacks salados"] = 33,
            ["dulces y chocolates"] = 34,
            ["helados y postres"] = 35,
            ["productos de limpieza"] = 36,
            ["desinfectantes"] = 37,
            ["detergentes y jabones"] = 38,
            ["suavizantes"] = 39,
            ["higiene personal"] = 40,
            ["cuidado bucal"] = 41,
            ["cuidado del cabello"] = 42,
            ["cuidado de la piel"] = 43,
            ["medicamentos y suplementos"] = 44,
            ["alimentos para mascotas"] = 45,
            ["otros"] = 46,

            // === Sinónimos útiles (IA) ===
            ["cereales"] = 1,
            ["granos"] = 1,
            ["pasta"] = 2,
            ["fideos"] = 2,
            ["frijoles"] = 4,
            ["legumbres"] = 4,
            ["pan"] = 5,
            ["tortillas"] = 5,
            ["galletas"] = 6,
            ["reposteria"] = 6,
            ["aceite"] = 7,
            ["grasas"] = 7,
            ["azucar"] = 8,
            ["endulzantes"] = 8,
            ["especias"] = 9,
            ["sal"] = 9,
            ["salsas"] = 10,
            ["condimentos"] = 10,
            ["sopas"] = 11,
            ["caldos"] = 11,
            ["verduras enlatadas"] = 12,
            ["frutas enlatadas"] = 13,
            ["pescado enlatado"] = 14,
            ["atun enlatado"] = 14,
            ["res"] = 15,
            ["carne"] = 15,
            ["pollo"] = 16,
            ["aves"] = 16,
            ["pescado"] = 17,
            ["mariscos"] = 17,
            ["embutidos"] = 18,
            ["fiambres"] = 18,
            ["lácteos"] = 19,
            ["leche"] = 19,
            ["queso"] = 21,
            ["yogurt"] = 22,
            ["yoghurt"] = 22,
            ["vino"] = 23,
            ["cerveza"] = 23,
            ["alcohol"] = 23,
            ["jugos"] = 24,
            ["néctares"] = 24,
            ["nectares"] = 24,
            ["agua"] = 25,
            ["energéticas"] = 26,
            ["energéticas"] = 26,
            ["energy drink"] = 26,
            ["café"] = 27,
            ["té"] = 28,
            ["infusiones"] = 28,
            ["cacao en polvo"] = 29,
            ["frutas"] = 30,
            ["verduras"] = 31,
            ["congelado"] = 32,
            ["congelados"] = 32,
            ["snack"] = 33,
            ["papas fritas"] = 33,
            ["dulces"] = 34,
            ["chocolates"] = 34,
            ["nutella"] = 34,
            ["helados"] = 35,
            ["postres"] = 35,
            ["limpieza"] = 36,
            ["desinfectante"] = 37,
            ["detergente"] = 38,
            ["jabones"] = 38,
            ["suavizante"] = 39,
            ["higiene"] = 40,
            ["higiene personal"] = 40,
            ["bucal"] = 41,
            ["pasta dental"] = 41,
            ["cabello"] = 42,
            ["shampoo"] = 42,
            ["acondicionador"] = 42,
            ["piel"] = 43,
            ["crema corporal"] = 43,
            ["medicamentos"] = 44,
            ["suplementos"] = 44,
            ["mascotas"] = 45,
            ["perros"] = 45,
            ["gatos"] = 45
        };

        public static int Map(string? nombreCategoria)
        {
            const int OTROS = 46;
            if (string.IsNullOrWhiteSpace(nombreCategoria))
                return OTROS;

            var key = Normalize(nombreCategoria);

            // match exacto
            if (_map.TryGetValue(key, out var id))
                return id;

            // match por contiene (keywords)
            foreach (var kv in _map)
                if (key.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;

            return OTROS;
        }

        private static string Normalize(string s)
        {
            s = s.Trim().ToLowerInvariant();

            // quitar acentos
            var norm = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(capacity: norm.Length);
            foreach (var ch in norm)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

    }
}