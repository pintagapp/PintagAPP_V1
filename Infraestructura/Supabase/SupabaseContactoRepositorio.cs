using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Networking;
using PintagAPP.CasosUso.Interfaces;

namespace PintagAPP.Infraestructura.Supabase
{
    // Cada vez que alguien toca WhatsApp o Llamar queda registrado (sirve para calificar y para estadísticas).
    // Si no hay internet, el contacto queda pendiente en el celular y se envía cuando vuelva la conexión.
    public class SupabaseContactoRepositorio : IContactoRepositorio
    {
        private static readonly string ArchivoPendientes =
            Path.Combine(SupabaseHttp.CarpetaPrivada, "contactos_pendientes.json");

        private readonly SupabaseHttp _http;

        public SupabaseContactoRepositorio(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task RegistrarAsync(Guid usuarioId, long perfilId, string canal)
        {
            var pendientes = await LeerPendientesAsync();
            pendientes.Add(new ContactoPendiente { UsuarioId = usuarioId, PerfilId = perfilId, Canal = canal });
            await GuardarPendientesAsync(pendientes);

            await EnviarPendientesAsync();   // con internet se envía de inmediato
        }

        public async Task<List<long>> ObtenerPerfilesContactadosAsync(Guid usuarioId)
        {
            await EnviarPendientesAsync();   // primero se suben los contactos hechos sin internet

            var filas = await _http.Cliente.GetFromJsonAsync<List<SoloPerfilIdDto>>(
                $"rest/v1/contactos?usuario_id=eq.{usuarioId}&select=perfil_id");

            return (filas ?? new List<SoloPerfilIdDto>()).Select(f => f.PerfilId).Distinct().ToList();
        }

        // ---------- PENDIENTES (modo offline) ----------
        private async Task EnviarPendientesAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

            var pendientes = await LeerPendientesAsync();
            if (pendientes.Count == 0) return;

            var quedan = new List<ContactoPendiente>();
            foreach (var c in pendientes)
            {
                try
                {
                    var respuesta = await _http.Cliente.PostAsJsonAsync("rest/v1/contactos",
                        new { usuario_id = c.UsuarioId, perfil_id = c.PerfilId, canal = c.Canal });

                    // 401 = la sesión aún no se renueva: se reintenta después.
                    // Otro rechazo (por ejemplo, el perfil ya no existe): se descarta.
                    if ((int)respuesta.StatusCode == 401)
                        quedan.Add(c);
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    quedan.Add(c);   // se cayó la red: queda para la próxima
                }
            }

            await GuardarPendientesAsync(quedan);
        }

        private static async Task<List<ContactoPendiente>> LeerPendientesAsync()
        {
            try
            {
                if (!File.Exists(ArchivoPendientes)) return new List<ContactoPendiente>();
                var json = await File.ReadAllTextAsync(ArchivoPendientes);
                return JsonSerializer.Deserialize<List<ContactoPendiente>>(json) ?? new List<ContactoPendiente>();
            }
            catch (Exception)
            {
                return new List<ContactoPendiente>();
            }
        }

        private static async Task GuardarPendientesAsync(List<ContactoPendiente> lista)
        {
            try
            {
                Directory.CreateDirectory(SupabaseHttp.CarpetaPrivada);
                await File.WriteAllTextAsync(ArchivoPendientes, JsonSerializer.Serialize(lista));
            }
            catch (IOException)
            {
                // Si no se puede guardar, no se interrumpe la llamada ni el WhatsApp
            }
        }

        private class ContactoPendiente
        {
            public Guid UsuarioId { get; set; }
            public long PerfilId { get; set; }
            public string Canal { get; set; } = "";
        }
    }
}