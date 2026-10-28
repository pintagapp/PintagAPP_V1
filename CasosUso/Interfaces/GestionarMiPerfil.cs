using System.Text.RegularExpressions;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Perfiles
{
    public class GestionarMiPerfil
    {
        private readonly IPerfilRepositorio _perfiles;
        private readonly ICatalogoRepositorio _catalogo;
        private readonly IAlmacenamientoServicio _almacenamiento;
        private readonly SesionActual _sesion;

        public GestionarMiPerfil(IPerfilRepositorio perfiles, ICatalogoRepositorio catalogo,
                                 IAlmacenamientoServicio almacenamiento, SesionActual sesion)
        {
            _perfiles = perfiles;
            _catalogo = catalogo;
            _almacenamiento = almacenamiento;
            _sesion = sesion;
        }

        public Task<List<Barrio>> ObtenerBarriosAsync() => _catalogo.ObtenerBarriosAsync();

        public async Task<PerfilOferente?> ObtenerAsync()
        {
            if (!_sesion.EstaIniciada) return null;
            return await _perfiles.ObtenerPorUsuarioAsync(_sesion.Usuario!.Id);
        }

        public async Task<Resultado> GuardarAsync(PerfilOferente perfil, byte[]? fotoNueva, bool autorizaPublicacion)
        {
            if (!_sesion.EstaIniciada)
                return Resultado.Error("Inicia sesión para continuar.");

            perfil.Telefono = (perfil.Telefono ?? "").Trim();
            perfil.Descripcion = (perfil.Descripcion ?? "").Trim();
            perfil.NombreNegocio = string.IsNullOrWhiteSpace(perfil.NombreNegocio) ? null : perfil.NombreNegocio.Trim();

            if (!Regex.IsMatch(perfil.Telefono, @"^[0-9+ ]{7,15}$"))
                return Resultado.Error("Escribe un teléfono válido (solo números, de 7 a 15 dígitos).");
            if (perfil.BarrioId == 0)
                return Resultado.Error("Elige tu barrio o comunidad.");
            if (perfil.Descripcion.Length == 0)
                return Resultado.Error("Cuéntale a la comunidad a qué te dedicas.");
            if (perfil.Descripcion.Length > 500)
                return Resultado.Error("La descripción puede tener máximo 500 caracteres.");
            if (perfil.NombreNegocio?.Length > 80)
                return Resultado.Error("El nombre del negocio puede tener máximo 80 caracteres.");
            if (perfil.EsNuevo && !autorizaPublicacion)
                return Resultado.Error("Debes autorizar la publicación de tus datos para continuar.");

            bool eraNuevo = perfil.EsNuevo;
            perfil.UsuarioId = _sesion.Usuario!.Id;

            try
            {
                if (fotoNueva is not null)
                    perfil.FotoUrl = await _almacenamiento.SubirImagenAsync($"{perfil.UsuarioId}/perfil.jpg", fotoNueva);

                await _perfiles.GuardarAsync(perfil);

                return Resultado.Exito(eraNuevo
                    ? "¡Listo! Tu perfil fue enviado a revisión. Puedes ver su estado en Mi perfil."
                    : "Cambios guardados.");
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo guardar. Revisa tu conexión e inténtalo de nuevo.");
            }
        }
    }
}
