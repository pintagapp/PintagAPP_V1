using Android.App.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace PintagAPP.Dominio.Entidades
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string NombresCompletos { get; set; } = "";

        public string Rol { get; set; } = "usuario";
        public bool EsAdmin => Rol == "admin";
    }
}
