using System.Net.Http.Headers;
using PintagAPP.CasosUso.Interfaces;
using System.Net.Http.Json;

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
        public async Task EliminarImagenAsync(string url)
        {
            // De la URL pública saca la ruta dentro del bucket: <usuario>/oferta1_foto1.jpg
            const string marcador = "/storage/v1/object/public/fotos/";
            int inicio = url.IndexOf(marcador);
            if (inicio < 0) return;

            var ruta = url[(inicio + marcador.Length)..];
            int consulta = ruta.IndexOf('?');
            if (consulta >= 0) ruta = ruta[..consulta];

            // Si falla no es grave: la foto ya no está enlazada a ninguna oferta
            await _http.Cliente.DeleteAsync($"storage/v1/object/fotos/{ruta}");
        }


        // Borra todas las fotos de una carpeta: fotos/<id_usuario>/ (al eliminar la cuenta)
        public async Task EliminarCarpetaAsync(string carpeta)
        {
            var lista = await _http.Cliente.PostAsJsonAsync("storage/v1/object/list/fotos",
                new { prefix = carpeta, limit = 1000, offset = 0 });
            if (!lista.IsSuccessStatusCode) return;

            var archivos = await lista.Content.ReadFromJsonAsync<List<ArchivoStorageDto>>() ?? new();
            var rutas = archivos.Where(a => a.Id is not null).Select(a => $"{carpeta}/{a.Nombre}").ToList();
            if (rutas.Count == 0) return;

            using var solicitud = new HttpRequestMessage(HttpMethod.Delete, "storage/v1/object/fotos")
            {
                Content = JsonContent.Create(new { prefixes = rutas })
            };
            await _http.Cliente.SendAsync(solicitud);
        }
    }
}