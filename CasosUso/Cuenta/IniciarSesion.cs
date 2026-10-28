using System;
using System.Collections.Generic;
using System.Text;
using PintagAPP.CasosUso.Interfaces;

namespace PintagAPP.CasosUso.Cuenta
{
    public class IniciarSesion
    {
        private readonly IAuthServicio _auth;
        public IniciarSesion(IAuthServicio auth)
        {
            _auth = auth;
        }
        public async Task<Resultado> EjecutarAsync(string correo, string contrasena)
        {
            correo = correo?.Trim().ToLower() ?? "";
            if (correo.Length == 0 || string.IsNullOrEmpty(contrasena))
                return Resultado.Error("Escribe tu correo y tu contraseña.");
            return await _auth.IniciarSesionAsync(correo, contrasena);
            {

            }
        }
    }
}
