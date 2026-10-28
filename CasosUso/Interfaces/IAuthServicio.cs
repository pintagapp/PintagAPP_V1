using System;
using System.Collections.Generic;
using System.Text;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface IAuthServicio
    {
        Task<Resultado> RegistrarAsync(string correo, string contrasena, string cedula, string nombresCompletos);
    }
}
