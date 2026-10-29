using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface IOfertaRepositorio
    {
        Task<List<Oferta>> ObtenerDePerfilAsync(long perfilId);
        Task<Oferta> GuardarAsync(Oferta oferta);
        Task EliminarAsync(long ofertaId);
        Task AgregarFotoAsync(long ofertaId, string url, int orden);
        Task EliminarFotoAsync(long fotoId);
    }
}