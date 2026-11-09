using System.Net.Http.Json;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseDenunciaRepositorio : IDenunciaRepositorio
    {
        // Trae la denuncia con lo denunciado: la oferta, la opinión o el perfil, y el nombre del oferente
        private const string ColumnasAdmin =
            "id,motivo,fecha,perfil_id,oferta_id,calificacion_id," +
            "ofertas(titulo,perfil_id,perfiles_oferente(nombre_persona,nombre_negocio))," +
            "calificaciones(estrellas,comentario,perfiles_oferente(nombre_persona,nombre_negocio))," +
            "perfiles_oferente(nombre_persona,nombre_negocio)";

        private readonly SupabaseHttp _http;

        public SupabaseDenunciaRepositorio(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task CrearAsync(Denuncia d)
        {
            var respuesta = await _http.Cliente.PostAsJsonAsync("rest/v1/denuncias", new
            {
                usuario_id = d.UsuarioId,
                perfil_id = d.PerfilId,
                oferta_id = d.OfertaId,
                calificacion_id = d.CalificacionId,
                motivo = d.Motivo
            });
            respuesta.EnsureSuccessStatusCode();
        }

        public async Task<List<Denuncia>> ObtenerPendientesAsync()
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<DenunciaDto>>(
                $"rest/v1/denuncias?estado=eq.pendiente&select={ColumnasAdmin}&order=fecha.asc");

            return (filas ?? new List<DenunciaDto>()).Select(Mapear).ToList();
        }

        public async Task MarcarRevisadaAsync(long denunciaId)
        {
            var respuesta = await _http.Cliente.PatchAsJsonAsync(
                $"rest/v1/denuncias?id=eq.{denunciaId}", new { estado = "revisado" });
            respuesta.EnsureSuccessStatusCode();
        }

        private static Denuncia Mapear(DenunciaDto f)
        {
            var d = new Denuncia
            {
                Id = f.Id,
                Motivo = f.Motivo,
                Fecha = f.Fecha,
                PerfilId = f.PerfilId,
                OfertaId = f.OfertaId,
                CalificacionId = f.CalificacionId
            };

            if (f.Oferta is not null)
            {
                d.DescripcionObjetivo = $"Oferta «{f.Oferta.Titulo}» de {Nombre(f.Oferta.Perfil)}";
                d.PerfilAfectadoId = f.Oferta.PerfilId;
            }
            else if (f.Calificacion is not null)
            {
                var estrellas = new string('★', f.Calificacion.Estrellas);
                var comentario = string.IsNullOrWhiteSpace(f.Calificacion.Comentario)
                    ? "(sin comentario)"
                    : $"«{f.Calificacion.Comentario}»";
                d.DescripcionObjetivo = $"Opinión {estrellas} sobre {Nombre(f.Calificacion.Perfil)}: {comentario}";
            }
            else if (f.Perfil is not null)
            {
                d.DescripcionObjetivo = $"Perfil de {Nombre(f.Perfil)}";
                d.PerfilAfectadoId = f.PerfilId;
            }
            else
            {
                d.DescripcionObjetivo = "(ya no existe)";
            }

            return d;
        }

        private static string Nombre(PerfilDto? p) =>
            p is null ? "?" : string.IsNullOrWhiteSpace(p.NombreNegocio) ? p.NombrePersona : p.NombreNegocio!;
    }
}