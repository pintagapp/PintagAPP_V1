using System;
using System.Collections.Generic;
using System.Text;

namespace PintagAPP.Dominio.Entidades
{
    public class Categoria
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public string Icono { get; set;  }
        public int Orden { get; set; }
        public override string ToString() => Nombre;
    }
}
