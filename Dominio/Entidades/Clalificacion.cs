namespace PintagAPP.Dominio.Entidades
{
    public class Calificacion
    {
        public long Id { get; set; }
        public Guid UsuarioId { get; set; }
        public long PerfilId { get; set; }
        public int Estrellas { get; set; }
        public string? Comentario { get; set; }
        public DateTimeOffset Fecha { get; set; }

        public bool EsNueva => Id == 0;

        // ★★★★☆
        public string TextoEstrellas => new string('★', Estrellas) + new string('☆', 5 - Estrellas);
    }
}