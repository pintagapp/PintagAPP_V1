using System.Globalization;

namespace PintagAPP.Dominio.Entidades
{
    public class Oferta
    {
        public long Id { get; set; }
        public long PerfilId { get; set; }
        public int Posicion { get; set; }
        public long CategoriaId { get; set; }
        public string Tipo { get; set; } = "servicio";   // "producto" o "servicio"
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int? UnidadesPorPrecio { get; set; }
        public decimal? Precio { get; set; }
        public bool AConvenir { get; set; }
        public bool Activa { get; set; } = true;
        public List<FotoOferta> Fotos { get; set; } = new();

        public bool EsNueva => Id == 0;
        public bool EsProducto => Tipo == "producto";

        // Regla de presentación del precio: "8 por $1.00", "$15.00" o "A convenir"
        public string TextoPrecio
        {
            get
            {
                if (AConvenir || Precio is null) return "A convenir";
                var precio = "$" + Precio.Value.ToString("0.00", CultureInfo.InvariantCulture);
                return UnidadesPorPrecio is > 1 ? $"{UnidadesPorPrecio} por {precio}" : precio;
            }
        }
    }
}
