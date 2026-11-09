using System.Net.Http.Json;
using PintagAPP.CasosUso.Interfaces;

namespace PintagAPP.Infraestructura.Supabase
{
    // Cada vez que alguien toca WhatsApp o Llamar queda registrado (sirve para calificar y para estadísticas)
    public class SupabaseContactoRepositorio : IContactoRepositorio
    {
        private readonly SupabaseHttp _http;

        public SupabaseContactoRepositorio(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task RegistrarAsync(Guid usuarioId, long perfilId, string canal)
        {
            var respuesta = await _http.Cliente.PostAsJsonAsync("rest/v1/contactos",
                new { usuario_id = usuarioId, perfil_id = perfilId, canal });
            respuesta.EnsureSuccessStatusCode();
        }

        public async Task<List<long>> ObtenerPerfilesContactadosAsync(Guid usuarioId)
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<SoloPerfilIdDto>>(
                $"rest/v1/contactos?usuario_id=eq.{usuarioId}&select=perfil_id");

            return (filas ?? new List<SoloPerfilIdDto>()).Select(f => f.PerfilId).Distinct().ToList();
        }
    }
}
