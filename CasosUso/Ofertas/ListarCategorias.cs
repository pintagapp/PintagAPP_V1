using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Ofertas
{
    public class ListarCategorias
    {
        private readonly ICatalogoRepositorio _repositorio;

        public ListarCategorias(ICatalogoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<Categoria>> EjecutarAsync() => _repositorio.ObtenerCategoriasAsync();
    }
}