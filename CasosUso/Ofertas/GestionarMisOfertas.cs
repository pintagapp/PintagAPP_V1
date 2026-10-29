using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Ofertas
{
    public class GestionarMisOfertas
    {
        public const int MaximoOfertas = 2;
        public const int MaximoFotos = 5;

        private readonly IOfertaRepositorio _ofertas;
        private readonly IPerfilRepositorio _perfiles;
        private readonly ICatalogoRepositorio _catalogo;
        private readonly IAlmacenamientoServicio _almacenamiento;
        private readonly SesionActual _sesion;

        public GestionarMisOfertas(IOfertaRepositorio ofertas, IPerfilRepositorio perfiles,
                                   ICatalogoRepositorio catalogo, IAlmacenamientoServicio almacenamiento,
                                   SesionActual sesion)
        {
            _ofertas = ofertas;
            _perfiles = perfiles;
            _catalogo = catalogo;
            _almacenamiento = almacenamiento;
            _sesion = sesion;
        }

        public Task<List<Categoria>> ObtenerCategoriasAsync() => _catalogo.ObtenerCategoriasAsync();

        public async Task<PerfilOferente?> ObtenerMiPerfilAsync()
        {
            if (!_sesion.EstaIniciada) return null;
            return await _perfiles.ObtenerPorUsuarioAsync(_sesion.Usuario!.Id);
        }

        public Task<List<Oferta>> ObtenerOfertasAsync(long perfilId) => _ofertas.ObtenerDePerfilAsync(perfilId);

        public async Task<Resultado> GuardarAsync(Oferta oferta, List<FotoOferta> fotosAEliminar, List<byte[]> fotosNuevas)
        {
            // ---- Reglas de negocio ----
            oferta.Titulo = (oferta.Titulo ?? "").Trim();
            oferta.Descripcion = (oferta.Descripcion ?? "").Trim();

            if (oferta.Tipo != "producto" && oferta.Tipo != "servicio")
                return Resultado.Error("Elige si es un producto o un servicio.");
            if (oferta.CategoriaId == 0)
                return Resultado.Error("Elige una categoría.");
            if (oferta.Titulo.Length < 3 || oferta.Titulo.Length > 60)
                return Resultado.Error("El título debe tener entre 3 y 60 caracteres.");
            if (oferta.Descripcion.Length == 0 || oferta.Descripcion.Length > 500)
                return Resultado.Error("Escribe una descripción de hasta 500 caracteres.");

            if (oferta.AConvenir)
            {
                oferta.Precio = null;
                oferta.UnidadesPorPrecio = null;
            }
            else if (oferta.Precio is null || oferta.Precio <= 0)
            {
                return Resultado.Error("Escribe un precio mayor a 0 o marca \"A convenir\".");
            }

            if (!oferta.EsProducto)
                oferta.UnidadesPorPrecio = null;
            else if (oferta.UnidadesPorPrecio is < 1)
                return Resultado.Error("Las unidades deben ser 1 o más.");

            int fotosQueQuedan = oferta.Fotos.Count(f => !fotosAEliminar.Any(e => e.Id == f.Id));
            if (fotosQueQuedan + fotosNuevas.Count > MaximoFotos)
                return Resultado.Error($"Puedes tener máximo {MaximoFotos} fotos por oferta.");

            try
            {
                var perfil = await ObtenerMiPerfilAsync();
                if (perfil is null)
                    return Resultado.Error("Primero completa tu perfil en Mi negocio.");
                if (perfil.Estado == "bloqueado")
                    return Resultado.Error("Tu perfil está bloqueado. Revisa el motivo en Mi negocio.");

                bool eraNueva = oferta.EsNueva;
                oferta.PerfilId = perfil.Id;

                if (eraNueva)
                {
                    var existentes = await _ofertas.ObtenerDePerfilAsync(perfil.Id);
                    if (existentes.Count >= MaximoOfertas)
                        return Resultado.Error($"Puedes tener máximo {MaximoOfertas} ofertas.");
                    oferta.Posicion = existentes.Any(o => o.Posicion == 1) ? 2 : 1;
                }

                var guardada = await _ofertas.GuardarAsync(oferta);

                // 1) Quitar las fotos que el usuario eliminó
                foreach (var foto in fotosAEliminar)
                {
                    await _ofertas.EliminarFotoAsync(foto.Id);
                    await _almacenamiento.EliminarImagenAsync(foto.Url);
                }

                // 2) Subir las nuevas en las posiciones libres (1 a 5)
                var ocupadas = oferta.Fotos
                    .Where(f => !fotosAEliminar.Any(e => e.Id == f.Id))
                    .Select(f => f.Orden)
                    .ToHashSet();

                foreach (var bytes in fotosNuevas)
                {
                    int orden = Enumerable.Range(1, MaximoFotos).First(n => !ocupadas.Contains(n));
                    ocupadas.Add(orden);
                    var ruta = $"{_sesion.Usuario!.Id}/oferta{guardada.Posicion}_foto{orden}.jpg";
                    var url = await _almacenamiento.SubirImagenAsync(ruta, bytes);
                    await _ofertas.AgregarFotoAsync(guardada.Id, url, orden);
                }

                if (!eraNueva) return Resultado.Exito("Cambios guardados.");
                return Resultado.Exito(perfil.Estado == "aprobado"
                    ? "¡Oferta publicada!"
                    : "Oferta guardada. Se publicará cuando el administrador apruebe tu perfil.");
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo guardar la oferta. Revisa tu conexión e inténtalo de nuevo.");
            }
        }

        public async Task<Resultado> EliminarAsync(Oferta oferta)
        {
            try
            {
                foreach (var foto in oferta.Fotos)
                    await _almacenamiento.EliminarImagenAsync(foto.Url);

                await _ofertas.EliminarAsync(oferta.Id);   // las filas de fotos se borran en cascada
                return Resultado.Exito("Oferta eliminada.");
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo eliminar la oferta. Inténtalo de nuevo.");
            }
        }
    }
}