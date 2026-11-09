using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Ofertas
{
    public record DatosContacto(string Telefono, string NumeroWhatsApp);

    // Lo que hace cualquier persona: ver ofertas por categoría, su detalle y contactar al oferente
    public class ExplorarOfertas
    {
        public const string CanalWhatsApp = "whatsapp";
        public const string CanalLlamada = "llamada";

        private readonly IOfertaRepositorio _ofertas;
        private readonly IPerfilRepositorio _perfiles;
        private readonly IContactoRepositorio _contactos;
        private readonly SesionActual _sesion;

        public ExplorarOfertas(IOfertaRepositorio ofertas, IPerfilRepositorio perfiles,
                               IContactoRepositorio contactos, SesionActual sesion)
        {
            _ofertas = ofertas;
            _perfiles = perfiles;
            _contactos = contactos;
            _sesion = sesion;
        }

        public bool HaySesion => _sesion.EstaIniciada;

        public async Task<List<Oferta>> ListarPorCategoriaAsync(long categoriaId)
        {
            var ofertas = await _ofertas.ObtenerPublicasPorCategoriaAsync(categoriaId);
            await AgregarCalificacionesAsync(ofertas);
            return ofertas;
        }

        public async Task<Oferta?> ObtenerDetalleAsync(long ofertaId)
        {
            var oferta = await _ofertas.ObtenerPublicaAsync(ofertaId);
            if (oferta is not null)
                await AgregarCalificacionesAsync(new List<Oferta> { oferta });
            return oferta;
        }

        public const int MinimoBusqueda = 3;

        public async Task<List<Oferta>> BuscarAsync(string? texto)
        {
            texto = texto?.Trim() ?? "";
            if (texto.Length < MinimoBusqueda) return new List<Oferta>();

            var ofertas = await _ofertas.BuscarPublicasAsync(texto);
            await AgregarCalificacionesAsync(ofertas);
            return ofertas;
        }

        // Completa el promedio de estrellas de cada oferente (una sola consulta para toda la lista)
        private async Task AgregarCalificacionesAsync(List<Oferta> ofertas)
        {
            var perfiles = await _perfiles.ObtenerPublicosAsync(ofertas.Select(o => o.PerfilId));
            foreach (var o in ofertas)
            {
                var p = perfiles.FirstOrDefault(x => x.Id == o.PerfilId);
                if (o.Oferente is not null && p is not null)
                {
                    o.Oferente.PromedioEstrellas = p.PromedioEstrellas;
                    o.Oferente.TotalCalificaciones = p.TotalCalificaciones;
                }
            }
        }

        // ¿La oferta es del usuario que está viendo? (para no mostrarle los botones de contacto)
        public async Task<bool> EsMiaAsync(Oferta oferta)
        {
            if (!_sesion.EstaIniciada) return false;
            var miPerfil = await _perfiles.ObtenerPorUsuarioAsync(_sesion.Usuario!.Id);
            return miPerfil?.Id == oferta.PerfilId;
        }

        // Obtiene el teléfono (solo con sesión) y registra el contacto
        public async Task<(Resultado Resultado, DatosContacto? Datos)> ContactarAsync(Oferta oferta, string canal)
        {
            if (!_sesion.EstaIniciada)
                return (Resultado.Error("Inicia sesión para contactar al oferente."), null);

            try
            {
                var telefono = await _perfiles.ObtenerTelefonoAsync(oferta.PerfilId);
                if (string.IsNullOrWhiteSpace(telefono))
                    return (Resultado.Error("No se encontró el teléfono del oferente."), null);

                await _contactos.RegistrarAsync(_sesion.Usuario!.Id, oferta.PerfilId, canal);
                return (Resultado.Exito(), new DatosContacto(telefono.Trim(), NumeroInternacional(telefono)));
            }
            catch (Exception)
            {
                return (Resultado.Error("No se pudo obtener el contacto. Revisa tu conexión."), null);
            }
        }

        public static string MensajeWhatsApp(Oferta oferta) =>
            $"Hola, vi tu oferta «{oferta.Titulo}» en Píntag Conecta y quisiera más información.";

        // 0991234567 → 593991234567   |   +593 99 123 4567 → 593991234567
        public static string NumeroInternacional(string telefono)
        {
            var digitos = new string(telefono.Where(char.IsDigit).ToArray());
            if (digitos.StartsWith("593")) return digitos;
            if (digitos.StartsWith("0")) return "593" + digitos.Substring(1);
            return "593" + digitos;
        }
    }
}
