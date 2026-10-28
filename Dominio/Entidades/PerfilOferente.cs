namespace PintagAPP.Dominio.Entidades
{
    public class PerfilOferente
    {
        public long Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string NombrePersona { get; set; } = "";
        public string? NombreNegocio { get; set; }
        public string Telefono { get; set; } = "";
        public long BarrioId { get; set; }
        public string Descripcion { get; set; } = "";
        public string? FotoUrl { get; set; }
        public string Estado { get; set; } = "pendiente";
        public string? MotivoEstado { get; set; }

        public bool EsNuevo => Id == 0;
    }
}