using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface ICalificacionRepositorio
    {
        Task<List<Calificacion>> ObtenerRecientesAsync(long perfilId, int cantidad);
        Task<Calificacion?> ObtenerMiaAsync(Guid usuarioId, long perfilId);
        Task<List<long>> ObtenerPerfilesCalificadosAsync(Guid usuarioId);
        Task GuardarAsync(Calificacion calificacion);
        Task OcultarAsync(long calificacionId);   // solo admin
    }
}