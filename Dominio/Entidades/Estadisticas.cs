using System.Globalization;

namespace PintagAPP.Dominio.Entidades
{
    public class Estadisticas
    {
        public int TotalUsuarios { get; set; }
        public int OferentesAprobados { get; set; }
        public int OferentesPendientes { get; set; }
        public int OferentesBloqueados { get; set; }
        public int OfertasActivas { get; set; }
        public int OfertasProductos { get; set; }
        public int OfertasServicios { get; set; }
        public int TotalContactos { get; set; }
        public int TotalCalificaciones { get; set; }
        public decimal? PromedioGeneral { get; set; }
        public int DenunciasPendientes { get; set; }

        public string PromedioTexto => PromedioGeneral is null
            ? "—"
            : "⭐ " + PromedioGeneral.Value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    public class EstadisticaCategoria
    {
        public string Categoria { get; set; } = "";
        public int OfertasActivas { get; set; }
        public int Oferentes { get; set; }
    }
}
