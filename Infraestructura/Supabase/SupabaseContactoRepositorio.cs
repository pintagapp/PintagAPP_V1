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
    }
}
