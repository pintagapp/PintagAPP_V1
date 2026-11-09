namespace PintagAPP.Dominio.Entidades
{
    public class Noticia
    {
        public long Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Contenido { get; set; } = "";
        public string? ImagenUrl { get; set; }
        public DateTimeOffset FechaPublicacion { get; set; }
        public bool Activa { get; set; } = true;

        public bool EsNueva => Id == 0;
    }
}