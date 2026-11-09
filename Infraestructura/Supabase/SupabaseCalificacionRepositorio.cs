using System.Net.Http.Json;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseCalificacionRepositorio : ICalificacionRepositorio
    {
        // Sin usuario_id: las opiniones se muestran como "Usuario de Píntag"
        private const string Columnas = "id,perfil_id,estrellas,comentario,fecha";

        private readonly SupabaseHttp _http;

        public SupabaseCalificacionRepositorio(SupabaseHttp http)
        {
            _http = http;
        }

        // visible=eq.true: el admin también ve las ocultas por RLS, aquí solo se muestran las visibles
        public async Task<List<Calificacion>> ObtenerRecientesAsync(long perfilId, int cantidad)
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<CalificacionDto>>(
                $"rest/v1/calificaciones?perfil_id=eq.{perfilId}&visible=eq.true&select={Columnas}" +
                $"&order=fecha.desc&limit={cantidad}");

            return (filas ?? new List<CalificacionDto>()).Select(Mapear).ToList();
        }

        public async Task<Calificacion?> ObtenerMiaAsync(Guid usuarioId, long perfilId)
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<CalificacionDto>>(
                $"rest/v1/calificaciones?usuario_id=eq.{usuarioId}&perfil_id=eq.{perfilId}&select={Columnas}");

            var fila = filas?.FirstOrDefault();
            if (fila is null) return null;

            var calificacion = Mapear(fila);
            calificacion.UsuarioId = usuarioId;
            return calificacion;
        }

        public async Task<List<long>> ObtenerPerfilesCalificadosAsync(Guid usuarioId)
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<SoloPerfilIdDto>>(
                $"rest/v1/calificaciones?usuario_id=eq.{usuarioId}&select=perfil_id");

            return (filas ?? new List<SoloPerfilIdDto>()).Select(f => f.PerfilId).ToList();
        }

        // Nueva: la BD verifica que antes haya contactado al oferente (política calificaciones_crear)
        public async Task GuardarAsync(Calificacion c)
        {
            HttpResponseMessage respuesta;

            if (c.EsNueva)
            {
                respuesta = await _http.Cliente.PostAsJsonAsync("rest/v1/calificaciones", new
                {
                    usuario_id = c.UsuarioId,
                    perfil_id = c.PerfilId,
                    estrellas = c.Estrellas,
                    comentario = c.Comentario
                });
            }
            else
            {
                respuesta = await _http.Cliente.PatchAsJsonAsync($"rest/v1/calificaciones?id=eq.{c.Id}", new
                {
                    estrellas = c.Estrellas,
                    comentario = c.Comentario
                });
            }

            respuesta.EnsureSuccessStatusCode();
        }

        // Solo el admin puede cambiar "visible" (lo controla el trigger proteger_calificacion)
        public async Task OcultarAsync(long calificacionId)
        {
            var respuesta = await _http.Cliente.PatchAsJsonAsync(
                $"rest/v1/calificaciones?id=eq.{calificacionId}", new { visible = false });
            respuesta.EnsureSuccessStatusCode();
        }

        private static Calificacion Mapear(CalificacionDto f) => new()
        {
            Id = f.Id,
            PerfilId = f.PerfilId,
            Estrellas = f.Estrellas,
            Comentario = f.Comentario,
            Fecha = f.Fecha
        };
    }
}
