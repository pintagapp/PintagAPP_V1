using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Perfiles
{
    // Panel del administrador: revisar perfiles pendientes y aprobarlos o bloquearlos
    public class ModerarPerfiles
    {
        public const int MaximoMotivo = 300;

        private readonly IPerfilRepositorio _perfiles;
        private readonly IOfertaRepositorio _ofertas;
        private readonly SesionActual _sesion;

        public ModerarPerfiles(IPerfilRepositorio perfiles, IOfertaRepositorio ofertas, SesionActual sesion)
        {
            _perfiles = perfiles;
            _ofertas = ofertas;
            _sesion = sesion;
        }

        public bool EsAdmin => _sesion.Usuario?.EsAdmin ?? false;

        public async Task<List<PerfilOferente>> ObtenerPendientesAsync()
        {
            if (!EsAdmin) return new List<PerfilOferente>();
            return await _perfiles.ObtenerPorEstadoAsync("pendiente");
        }

        public async Task<int> ContarPendientesAsync()
        {
            var pendientes = await ObtenerPendientesAsync();
            return pendientes.Count;
        }

        public async Task<(PerfilOferente? Perfil, List<Oferta> Ofertas)> ObtenerDetalleAsync(long perfilId)
        {
            if (!EsAdmin) return (null, new List<Oferta>());

            var perfil = await _perfiles.ObtenerPorIdAsync(perfilId);
            if (perfil is null) return (null, new List<Oferta>());

            var ofertas = await _ofertas.ObtenerDePerfilAsync(perfilId);
            return (perfil, ofertas);
        }

        public async Task<Resultado> AprobarAsync(long perfilId)
        {
            if (!EsAdmin)
                return Resultado.Error("Solo el administrador puede aprobar perfiles.");

            try
            {
                await _perfiles.CambiarEstadoAsync(perfilId, "aprobado", null);
                return Resultado.Exito("Perfil aprobado. Ya es visible en la app junto con sus ofertas activas.");
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo aprobar el perfil. Revisa tu conexión e inténtalo de nuevo.");
            }
        }

        public async Task<Resultado> BloquearAsync(long perfilId, string? motivo)
        {
            if (!EsAdmin)
                return Resultado.Error("Solo el administrador puede bloquear perfiles.");

            motivo = (motivo ?? "").Trim();
            if (motivo.Length < 5)
                return Resultado.Error("Escribe el motivo del bloqueo (mínimo 5 caracteres). El oferente lo verá en Mi negocio.");
            if (motivo.Length > MaximoMotivo)
                return Resultado.Error($"El motivo puede tener máximo {MaximoMotivo} caracteres.");

            try
            {
                await _perfiles.CambiarEstadoAsync(perfilId, "bloqueado", motivo);
                return Resultado.Exito("Perfil bloqueado. El oferente verá el motivo en Mi negocio.");
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo bloquear el perfil. Revisa tu conexión e inténtalo de nuevo.");
            }
        }
    }
}
