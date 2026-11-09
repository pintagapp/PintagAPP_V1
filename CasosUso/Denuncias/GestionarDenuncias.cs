using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Denuncias
{
    // Usuarios: denunciar una oferta u opinión. Admin: revisarlas y actuar.
    public class GestionarDenuncias
    {
        public static readonly string[] Motivos =
            { "Información falsa", "Contenido ofensivo", "Posible estafa", "Otro" };
        public const int MaximoMotivo = 300;

        private readonly IDenunciaRepositorio _denuncias;
        private readonly IOfertaRepositorio _ofertas;
        private readonly ICalificacionRepositorio _calificaciones;
        private readonly IPerfilRepositorio _perfiles;
        private readonly SesionActual _sesion;

        public GestionarDenuncias(IDenunciaRepositorio denuncias, IOfertaRepositorio ofertas,
                                  ICalificacionRepositorio calificaciones, IPerfilRepositorio perfiles,
                                  SesionActual sesion)
        {
            _denuncias = denuncias;
            _ofertas = ofertas;
            _calificaciones = calificaciones;
            _perfiles = perfiles;
            _sesion = sesion;
        }

        public bool HaySesion => _sesion.EstaIniciada;
        public bool EsAdmin => _sesion.Usuario?.EsAdmin ?? false;

        // ---------- USUARIOS ----------
        public Task<Resultado> DenunciarOfertaAsync(long ofertaId, string motivo) =>
            CrearAsync(new Denuncia { OfertaId = ofertaId, Motivo = motivo });

        public Task<Resultado> DenunciarOpinionAsync(long calificacionId, string motivo) =>
            CrearAsync(new Denuncia { CalificacionId = calificacionId, Motivo = motivo });

        private async Task<Resultado> CrearAsync(Denuncia denuncia)
        {
            if (!HaySesion)
                return Resultado.Error("Inicia sesión para denunciar.");

            denuncia.Motivo = denuncia.Motivo.Trim();
            if (denuncia.Motivo.Length < 5)
                return Resultado.Error("Escribe el motivo de la denuncia (mínimo 5 caracteres).");
            if (denuncia.Motivo.Length > MaximoMotivo)
                denuncia.Motivo = denuncia.Motivo[..MaximoMotivo];

            try
            {
                denuncia.UsuarioId = _sesion.Usuario!.Id;
                await _denuncias.CrearAsync(denuncia);
                return Resultado.Exito("Gracias. El administrador revisará tu denuncia.");
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo enviar la denuncia. Revisa tu conexión.");
            }
        }

        // ---------- ADMINISTRADOR ----------
        public async Task<List<Denuncia>> ObtenerPendientesAsync()
        {
            if (!EsAdmin) return new List<Denuncia>();
            return await _denuncias.ObtenerPendientesAsync();
        }

        public Task<Resultado> OcultarOpinionAsync(Denuncia d) =>
            AtenderAsync(d, async () =>
            {
                if (d.CalificacionId is null) throw new InvalidOperationException();
                await _calificaciones.OcultarAsync(d.CalificacionId.Value);
            }, "Opinión ocultada. Ya no se muestra en la app.");

        public Task<Resultado> PausarOfertaAsync(Denuncia d) =>
            AtenderAsync(d, async () =>
            {
                if (d.OfertaId is null) throw new InvalidOperationException();
                await _ofertas.CambiarActivaAsync(d.OfertaId.Value, false);
            }, "Oferta pausada. Ya no se muestra al público.");

        public Task<Resultado> BloquearPerfilAsync(Denuncia d) =>
            AtenderAsync(d, async () =>
            {
                if (d.PerfilAfectadoId is null) throw new InvalidOperationException();
                var motivo = $"Bloqueado por denuncia: {d.Motivo}";
                if (motivo.Length > MaximoMotivo) motivo = motivo[..MaximoMotivo];
                await _perfiles.CambiarEstadoAsync(d.PerfilAfectadoId.Value, "bloqueado", motivo);
            }, "Perfil bloqueado. El oferente verá el motivo en Mi negocio.");

        public Task<Resultado> DescartarAsync(Denuncia d) =>
            AtenderAsync(d, () => Task.CompletedTask, "Denuncia descartada.");

        // Ejecuta la acción y marca la denuncia como revisada
        private async Task<Resultado> AtenderAsync(Denuncia d, Func<Task> accion, string mensajeExito)
        {
            if (!EsAdmin)
                return Resultado.Error("Solo el administrador puede atender denuncias.");

            try
            {
                await accion();
                await _denuncias.MarcarRevisadaAsync(d.Id);
                return Resultado.Exito(mensajeExito);
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo completar la acción. Revisa tu conexión.");
            }
        }
    }
}