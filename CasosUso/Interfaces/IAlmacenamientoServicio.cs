namespace PintagAPP.CasosUso.Interfaces
{
    public interface IAlmacenamientoServicio
    {
        // Sube una imagen y devuelve su URL pública
        Task<string> SubirImagenAsync(string ruta, byte[] contenido);
        Task EliminarImagenAsync(string url);
    }
}
