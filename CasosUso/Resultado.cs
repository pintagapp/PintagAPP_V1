using System;
using System.Collections.Generic;
using System.Text;

namespace PintagAPP.CasosUso
{
    public record Resultado(bool EsExito, string Mensaje)
    {
        public static Resultado Exito(string mensaje = "") => new Resultado(true, mensaje);
        public static Resultado Error(string mensaje) => new Resultado(false, mensaje);
    }
}
