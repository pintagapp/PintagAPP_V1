using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseHttp
    {
        public HttpClient Cliente { get; }

        // Copias para el modo offline:
        //  - CarpetaCache: datos públicos (categorías, ofertas, noticias...). Se conservan siempre.
        //  - CarpetaPrivada: datos del usuario (teléfonos, contactos pendientes). Se borran al cerrar sesión.
        public static readonly string CarpetaCache = Path.Combine(FileSystem.AppDataDirectory, "cache_datos");
        public static readonly string CarpetaPrivada = Path.Combine(CarpetaCache, "privado");

        private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

        public SupabaseHttp()
        {
            Cliente = new HttpClient { BaseAddress = new Uri(SupabaseConfig.Url + "/") };
            // La clave publishable va SIEMPRE en "apikey" (nunca en Authorization)
            Cliente.DefaultRequestHeaders.Add("apikey", SupabaseConfig.PublishableKey);
        }

        // Después del login: el token del usuario va en Authorization
        public void EstablecerToken(string? token)
        {
            Cliente.DefaultRequestHeaders.Authorization =
                token is null ? null : new AuthenticationHeaderValue("Bearer", token);
        }

        // ---------- MODO OFFLINE ----------
        // Igual que GetFromJsonAsync, pero guarda una copia JSON de la respuesta en el celular.
        // Si no hay internet (o Supabase no responde), devuelve la última copia guardada.
        public async Task<T?> ObtenerConCacheAsync<T>(string url, bool privado = false)
        {
            var carpeta = privado ? CarpetaPrivada : CarpetaCache;
            var archivo = Path.Combine(carpeta, NombreArchivo(url));

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var json = await Cliente.GetStringAsync(url);
                    await GuardarCopiaAsync(carpeta, archivo, json);
                    return JsonSerializer.Deserialize<T>(json, OpcionesJson);
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    // Falló la red o Supabase: se intenta con la copia guardada
                }
            }

            if (File.Exists(archivo))
                return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(archivo), OpcionesJson);

            throw new HttpRequestException("Sin conexión y sin datos guardados.");
        }

        // Al cerrar sesión o eliminar la cuenta: se borra solo lo privado (teléfonos y contactos pendientes)
        public void BorrarPrivado()
        {
            try
            {
                if (Directory.Exists(CarpetaPrivada))
                    Directory.Delete(CarpetaPrivada, true);
            }
            catch (IOException)
            {
                // Si no se puede borrar ahora, no afecta el cierre de sesión
            }
        }

        private static async Task GuardarCopiaAsync(string carpeta, string archivo, string json)
        {
            try
            {
                Directory.CreateDirectory(carpeta);
                await File.WriteAllTextAsync(archivo, json);
            }
            catch (IOException)
            {
                // Si no se puede guardar la copia, la app sigue funcionando normal
            }
        }

        // Cada consulta (URL) se guarda en su propio archivo, con un nombre fijo calculado de la URL
        private static string NombreArchivo(string url) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url))) + ".json";
    }
}