using System;
using System.Collections.Generic;
using System.Text;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface IAuthServicio
    {
        Task<Resultado> RegistrarAsync(string correo, string contrasena, string cedula, string nombresCompletos);

        Task<Resultado> IniciarSesionAsync(string correo, string contrasena);

        Task<bool> RestaurarSesionAsync();   // al abrir la app, si ya había iniciado sesión antes

        Task CerrarSesionAsync();
    }
}
