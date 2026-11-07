using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface IPerfilRepositorio
    {
        Task<PerfilOferente?> ObtenerPorUsuarioAsync(Guid usuarioId);
        Task<PerfilOferente> GuardarAsync(PerfilOferente perfil);

        // ---- Administrador ----
        Task<List<PerfilOferente>> ObtenerPorEstadoAsync(string estado);
        Task<PerfilOferente?> ObtenerPorIdAsync(long id);
        Task CambiarEstadoAsync(long id, string estado, string? motivo);
    }
}