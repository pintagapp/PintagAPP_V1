using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Calificaciones
{
    // Calificar a un oferente (1 a 5 estrellas + comentario) y saber a quién falta calificar
    public class CalificarOferentes
    {
        public const int MaximoComentario = 300;
        public const int OpinionesVisibles = 5;

        private readonly ICalificacionRepositorio _calificaciones;
        private readonly IContactoRepositorio _contactos;
        private readonly IPerfilRepositorio _perfiles;
        private readonly SesionActual _sesion;

        public CalificarOferentes(ICalificacionRepositorio calificaciones, IContactoRepositorio contactos,
                                  IPerfilRepositorio perfiles, SesionActual sesion)
        {
            _calificaciones = calificaciones;
            _contactos = contactos;
            _perfiles = perfiles;
            _sesion = sesion;
        }

        public bool HaySesion => _sesion.EstaIniciada;

        public Task<List<Calificacion>> ObtenerOpinionesAsync(long perfilId) =>
            _calificaciones.ObtenerRecientesAsync(perfilId, OpinionesVisibles);

        public async Task<Calificacion?> ObtenerMiCalificacionAsync(long perfilId)
        {
            if (!HaySesion) return null;
            return await _calificaciones.ObtenerMiaAsync(_sesion.Usuario!.Id, perfilId);
        }

        // Reglas: con sesión, no a sí mismo y solo si antes lo contactó desde la app
        public async Task<Resultado> VerificarPuedeCalificarAsync(long perfilId)
        {
            if (!HaySesion)
                return Resultado.Error("Inicia sesión para calificar.");

            try
            {
                var usuarioId = _sesion.Usuario!.Id;

                var miPerfil = await _perfiles.ObtenerPorUsuarioAsync(usuarioId);
                if (miPerfil?.Id == perfilId)
                    return Resultado.Error("No puedes calificar tu propio negocio.");

                var contactados = await _contactos.ObtenerPerfilesContactadosAsync(usuarioId);
                if (!contactados.Contains(perfilId))
                    return Resultado.Error("Primero contacta al oferente por WhatsApp o llamada para poder calificarlo.");

                return Resultado.Exito();
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo verificar. Revisa tu conexión.");
            }
        }

        public async Task<Resultado> GuardarAsync(long perfilId, int estrellas, string? comentario)
        {
            if (!HaySesion)
                return Resultado.Error("Inicia sesión para calificar.");
            if (estrellas < 1 || estrellas > 5)
                return Resultado.Error("Toca las estrellas para elegir de 1 a 5.");

            comentario = comentario?.Trim();
            if (string.IsNullOrEmpty(comentario)) comentario = null;
            if (comentario is not null && comentario.Length > MaximoComentario)
                return Resultado.Error($"El comentario puede tener máximo {MaximoComentario} caracteres.");

            try
            {
                var usuarioId = _sesion.Usuario!.Id;
                var existente = await _calificaciones.ObtenerMiaAsync(usuarioId, perfilId);

                var calificacion = existente ?? new Calificacion { UsuarioId = usuarioId, PerfilId = perfilId };
                calificacion.Estrellas = estrellas;
                calificacion.Comentario = comentario;

                await _calificaciones.GuardarAsync(calificacion);
                return Resultado.Exito(existente is null
                    ? "¡Gracias por tu calificación! Ayudas a otros vecinos de Píntag."
                    : "Tu calificación se actualizó.");
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo guardar. Recuerda que solo puedes calificar a quien contactaste desde la app.");
            }
        }

        // Oferentes que contacté y aún no califiqué
        public async Task<List<PerfilOferente>> ObtenerPorCalificarAsync()
        {
            if (!HaySesion) return new List<PerfilOferente>();

            var usuarioId = _sesion.Usuario!.Id;
            var contactados = await _contactos.ObtenerPerfilesContactadosAsync(usuarioId);
            var calificados = await _calificaciones.ObtenerPerfilesCalificadosAsync(usuarioId);

            var pendientes = contactados.Except(calificados).ToList();
            return await _perfiles.ObtenerPublicosAsync(pendientes);
        }
    }
}