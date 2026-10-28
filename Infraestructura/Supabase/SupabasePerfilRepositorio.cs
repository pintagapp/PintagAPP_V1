using System.Net.Http.Json;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabasePerfilRepositorio : IPerfilRepositorio
    {
        private const string Columnas =
            "id,usuario_id,nombre_persona,nombre_negocio,telefono,barrio_id,descripcion,foto_url,estado,motivo_estado";

        private readonly SupabaseHttp _http;

        public SupabasePerfilRepositorio(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task<PerfilOferente?> ObtenerPorUsuarioAsync(Guid usuarioId)
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<PerfilDto>>(
                $"rest/v1/perfiles_oferente?usuario_id=eq.{usuarioId}&select={Columnas}");

            var fila = filas?.FirstOrDefault();
            return fila is null ? null : Mapear(fila);
        }

        public async Task<PerfilOferente> GuardarAsync(PerfilOferente perfil)
        {
            // Solo los campos que el oferente puede escribir
            var datos = new Dictionary<string, object?>
            {
                ["nombre_negocio"] = perfil.NombreNegocio,
                ["telefono"] = perfil.Telefono,
                ["barrio_id"] = perfil.BarrioId,
                ["descripcion"] = perfil.Descripcion,
                ["foto_url"] = perfil.FotoUrl
            };

            HttpRequestMessage solicitud;

            if (perfil.EsNuevo)
            {
                datos["usuario_id"] = perfil.UsuarioId;
                datos["nombre_persona"] = "";   // el trigger de la BD pone el nombre real de la cuenta
                datos["autoriza_publicacion_version"] = SupabaseConfig.AutorizaPublicacionVersion;
                solicitud = new HttpRequestMessage(HttpMethod.Post, "rest/v1/perfiles_oferente");
            }
            else
            {
                solicitud = new HttpRequestMessage(HttpMethod.Patch, $"rest/v1/perfiles_oferente?id=eq.{perfil.Id}");
            }

            solicitud.Content = JsonContent.Create(datos);
            solicitud.Headers.Add("Prefer", "return=representation");   // que devuelva la fila guardada

            var respuesta = await _http.Cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            var filas = await respuesta.Content.ReadFromJsonAsync<List<PerfilDto>>();
            var guardado = Mapear(filas!.First());
            perfil.Id = guardado.Id;
            perfil.Estado = guardado.Estado;
            return guardado;
        }

        private static PerfilOferente Mapear(PerfilDto f) => new()
        {
            Id = f.Id,
            UsuarioId = f.UsuarioId,
            NombrePersona = f.NombrePersona,
            NombreNegocio = f.NombreNegocio,
            Telefono = f.Telefono,
            BarrioId = f.BarrioId,
            Descripcion = f.Descripcion,
            FotoUrl = f.FotoUrl,
            Estado = f.Estado,
            MotivoEstado = f.MotivoEstado
        };
    }
}
