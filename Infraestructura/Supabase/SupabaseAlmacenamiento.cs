using System.Net.Http.Headers;
using PintagAPP.CasosUso.Interfaces;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseAlmacenamiento : IAlmacenamientoServicio
    {
        private readonly SupabaseHttp _http;

        public SupabaseAlmacenamiento(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task<string> SubirImagenAsync(string ruta, byte[] contenido)
        {
            using var cuerpo = new ByteArrayContent(contenido);
            cuerpo.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            using var solicitud = new HttpRequestMessage(HttpMethod.Post, $"storage/v1/object/fotos/{ruta}")
            {
                Content = cuerpo
            };
            solicitud.Headers.Add("x-upsert", "true");   // si ya existe, la reemplaza

            var respuesta = await _http.Cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            // "?v=" evita que el celular muestre la foto anterior guardada en caché
            return $"{SupabaseConfig.Url}/storage/v1/object/public/fotos/{ruta}?v={DateTime.UtcNow.Ticks}";
        }
    }
}