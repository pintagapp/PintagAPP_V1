using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Noticias
{
    public class ListarNoticias
    {
        private readonly INoticiaRepositorio _repositorio;

        public ListarNoticias(INoticiaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<Noticia>> EjecutarAsync(int cantidad = 3) => _repositorio.ObtenerRecientesAsync(cantidad);
    }
}
