using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Perfiles
{
    public class VerEstadisticas
    {
        private readonly IEstadisticaRepositorio _repositorio;
        private readonly SesionActual _sesion;

        public VerEstadisticas(IEstadisticaRepositorio repositorio, SesionActual sesion)
        {
            _repositorio = repositorio;
            _sesion = sesion;
        }

        public bool EsAdmin => _sesion.Usuario?.EsAdmin ?? false;

        public async Task<Estadisticas?> ObtenerGeneralesAsync() =>
            EsAdmin ? await _repositorio.ObtenerGeneralesAsync() : null;

        // Las categorías con más ofertas primero
        public async Task<List<EstadisticaCategoria>> ObtenerPorCategoriaAsync()
        {
            if (!EsAdmin) return new List<EstadisticaCategoria>();
            var lista = await _repositorio.ObtenerPorCategoriaAsync();
            return lista.OrderByDescending(c => c.OfertasActivas).ThenBy(c => c.Categoria).ToList();
        }
    }
}