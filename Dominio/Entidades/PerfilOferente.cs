using System.Globalization;
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
        public string? BarrioNombre { get; set; }      // solo lectura: viene de la tabla barrios
        public string Descripcion { get; set; } = "";
        public string? FotoUrl { get; set; }
        public string Estado { get; set; } = "pendiente";
        public string? MotivoEstado { get; set; }
        public DateTimeOffset CreadoEn { get; set; }
        public decimal? PromedioEstrellas { get; set; }   // solo lectura: viene de vista_perfiles_publicos
        public int TotalCalificaciones { get; set; }

        public bool EsNuevo => Id == 0;

        // Cómo se muestra al público: el negocio si tiene, si no el nombre de la persona
        public string NombreVisible => string.IsNullOrWhiteSpace(NombreNegocio) ? NombrePersona : NombreNegocio;

        // "⭐ 4.5 (12)" o "Sin calificaciones"
        public string TextoCalificacion => TotalCalificaciones == 0 || PromedioEstrellas is null
            ? "Sin calificaciones"
            : $"⭐ {PromedioEstrellas.Value.ToString("0.0", CultureInfo.InvariantCulture)} ({TotalCalificaciones})";
    }
}