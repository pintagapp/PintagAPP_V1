using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Noticias
{
    public class AdministrarNoticias
    {
        private readonly INoticiaRepositorio _noticias;
        private readonly IAlmacenamientoServicio _almacenamiento;
        private readonly SesionActual _sesion;

        public AdministrarNoticias(INoticiaRepositorio noticias, IAlmacenamientoServicio almacenamiento,
                                   SesionActual sesion)
        {
            _noticias = noticias;
            _almacenamiento = almacenamiento;
            _sesion = sesion;
        }

        public bool EsAdmin => _sesion.Usuario?.EsAdmin ?? false;

        public async Task<List<Noticia>> ObtenerTodasAsync() =>
            EsAdmin ? await _noticias.ObtenerTodasAsync() : new List<Noticia>();

        public async Task<Resultado> GuardarAsync(Noticia noticia, byte[]? nuevaImagen)
        {
            if (!EsAdmin)
                return Resultado.Error("Solo el administrador puede publicar noticias.");

            noticia.Titulo = noticia.Titulo.Trim();
            noticia.Contenido = noticia.Contenido.Trim();
            if (noticia.Titulo.Length < 3 || noticia.Titulo.Length > 120)
                return Resultado.Error("El título debe tener entre 3 y 120 caracteres.");
            if (noticia.Contenido.Length < 10)
                return Resultado.Error("Escribe el contenido de la noticia (mínimo 10 caracteres).");

            try
            {
                // Las imágenes de noticias van en fotos/noticias/ (lo permite la política del bucket)
                if (nuevaImagen is not null)
                {
                    var anterior = noticia.ImagenUrl;
                    noticia.ImagenUrl = await _almacenamiento.SubirImagenAsync(
                        $"noticias/noticia_{DateTime.UtcNow.Ticks}.jpg", nuevaImagen);
                    if (anterior is not null)
                        await _almacenamiento.EliminarImagenAsync(anterior);
                }

                bool esNueva = noticia.EsNueva;
                await _noticias.GuardarAsync(noticia);
                return Resultado.Exito(esNueva ? "Noticia publicada." : "Noticia actualizada.");
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo guardar la noticia. Revisa tu conexión.");
            }
        }

        public async Task<Resultado> EliminarAsync(Noticia noticia)
        {
            if (!EsAdmin)
                return Resultado.Error("Solo el administrador puede eliminar noticias.");

            try
            {
                await _noticias.EliminarAsync(noticia.Id);
                if (noticia.ImagenUrl is not null)
                    await _almacenamiento.EliminarImagenAsync(noticia.ImagenUrl);
                return Resultado.Exito("Noticia eliminada.");
            }
            catch (Exception)
            {
                return Resultado.Error("No se pudo eliminar la noticia. Revisa tu conexión.");
            }
        }
    }
}