using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface INoticiaRepositorio
    {
        Task<List<Noticia>> ObtenerRecientesAsync(int cantidad);
    }
}
