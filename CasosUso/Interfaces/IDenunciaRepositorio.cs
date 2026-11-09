using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface IDenunciaRepositorio
    {
        Task CrearAsync(Denuncia denuncia);
        Task<List<Denuncia>> ObtenerPendientesAsync();   // solo admin
        Task MarcarRevisadaAsync(long denunciaId);       // solo admin
    }
}