using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface IPerfilRepositorio
    {
        Task<PerfilOferente?> ObtenerPorUsuarioAsync(Guid usuarioId);
        Task<PerfilOferente> GuardarAsync(PerfilOferente perfil);
    }
}