using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface IEstadisticaRepositorio
    {
        Task<Estadisticas?> ObtenerGeneralesAsync();
        Task<List<EstadisticaCategoria>> ObtenerPorCategoriaAsync();
    }
}