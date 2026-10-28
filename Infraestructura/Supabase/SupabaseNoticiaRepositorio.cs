using System.Net.Http.Json;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseNoticiaRepositorio : INoticiaRepositorio
    {
        private readonly SupabaseHttp _http;

        public SupabaseNoticiaRepositorio(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task<List<Noticia>> ObtenerRecientesAsync(int cantidad)
        {
            // Las noticias vencidas las filtra la política RLS de la base de datos
            var filas = await _http.Cliente.GetFromJsonAsync<List<NoticiaDto>>(
                "rest/v1/noticias?select=id,titulo,contenido,imagen_url,fecha_publicacion" +
                $"&activa=eq.true&order=fecha_publicacion.desc&limit={cantidad}");

            return (filas ?? new List<NoticiaDto>())
                .Select(f => new Noticia
                {
                    Id = f.Id,
                    Titulo = f.Titulo,
                    Contenido = f.Contenido,
                    ImagenUrl = f.ImagenUrl,
                    FechaPublicacion = f.FechaPublicacion
                })
                .ToList();
        }
    }
}
