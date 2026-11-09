using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface INoticiaRepositorio
    {
        Task<List<Noticia>> ObtenerRecientesAsync(int cantidad);

        // ---- Administrador ----
        Task<List<Noticia>> ObtenerTodasAsync();
        Task GuardarAsync(Noticia noticia);
        Task EliminarAsync(long noticiaId);
    }
}