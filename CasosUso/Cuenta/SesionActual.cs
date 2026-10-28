using System;
using System.Collections.Generic;
using System.Text;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.CasosUso.Cuenta
{
    public class SesionActual
    {
        public Usuario? Usuario { get; private set; }
        public bool EstaIniciada => Usuario is not null;
        public void Iniciar(Usuario usuario) => Usuario = usuario;
        public void Cerrar() => Usuario = null;
    }
}
