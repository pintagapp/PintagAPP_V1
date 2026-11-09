namespace PintagAPP.Dominio.Entidades
{
    public class Denuncia
    {
        public long Id { get; set; }
        public Guid UsuarioId { get; set; }
        public long? PerfilId { get; set; }
        public long? OfertaId { get; set; }
        public long? CalificacionId { get; set; }
        public string Motivo { get; set; } = "";
        public string Estado { get; set; } = "pendiente";
        public DateTimeOffset Fecha { get; set; }

        // Solo lectura (panel del admin): qué se denunció y a qué perfil afecta
        public string DescripcionObjetivo { get; set; } = "";
        public long? PerfilAfectadoId { get; set; }

        // Una denuncia apunta exactamente a una cosa (lo exige la BD)
        public string Tipo => OfertaId is not null ? "oferta"
                            : CalificacionId is not null ? "opinion"
                            : "perfil";
    }
}
