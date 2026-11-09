using System.Net.Http.Json;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseNoticiaRepositorio : INoticiaRepositorio
    {
        private const string Columnas = "id,titulo,contenido,imagen_url,fecha_publicacion,activa";

        private readonly SupabaseHttp _http;

        public SupabaseNoticiaRepositorio(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task<List<Noticia>> ObtenerRecientesAsync(int cantidad)
        {
            // Las noticias vencidas las filtra la política RLS de la base de datos
            var filas = await _http.Cliente.GetFromJsonAsync<List<NoticiaDto>>(
                $"rest/v1/noticias?select={Columnas}&activa=eq.true&order=fecha_publicacion.desc&limit={cantidad}");

            return (filas ?? new List<NoticiaDto>()).Select(Mapear).ToList();
        }

        // ---------- ADMINISTRADOR (RLS: solo el admin escribe y ve las ocultas) ----------
        public async Task<List<Noticia>> ObtenerTodasAsync()
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<NoticiaDto>>(
                $"rest/v1/noticias?select={Columnas}&order=fecha_publicacion.desc");

            return (filas ?? new List<NoticiaDto>()).Select(Mapear).ToList();
        }

        public async Task GuardarAsync(Noticia n)
        {
            var datos = new
            {
                titulo = n.Titulo,
                contenido = n.Contenido,
                imagen_url = n.ImagenUrl,
                activa = n.Activa
            };

            var respuesta = n.EsNueva
                ? await _http.Cliente.PostAsJsonAsync("rest/v1/noticias", datos)
                : await _http.Cliente.PatchAsJsonAsync($"rest/v1/noticias?id=eq.{n.Id}", datos);
            respuesta.EnsureSuccessStatusCode();
        }

        public async Task EliminarAsync(long noticiaId)
        {
            var respuesta = await _http.Cliente.DeleteAsync($"rest/v1/noticias?id=eq.{noticiaId}");
            respuesta.EnsureSuccessStatusCode();
        }

        private static Noticia Mapear(NoticiaDto f) => new()
        {
            Id = f.Id,
            Titulo = f.Titulo,
            Contenido = f.Contenido,
            ImagenUrl = f.ImagenUrl,
            FechaPublicacion = f.FechaPublicacion,
            Activa = f.Activa
        };
    }
}