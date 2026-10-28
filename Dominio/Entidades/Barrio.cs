namespace PintagAPP.Dominio.Entidades
{
    public class Barrio
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = "";

        public override string ToString() => Nombre;   // lo que muestra la lista desplegable
    }
}