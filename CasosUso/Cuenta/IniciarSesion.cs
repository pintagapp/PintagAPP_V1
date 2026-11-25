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
        }

        public async Task<Resultado> RecuperarContrasenaAsync(string correo)
        {
            correo = correo?.Trim().ToLower() ?? "";
            if (!correo.Contains('@') || !correo.Contains('.'))
                return Resultado.Error("Escribe un correo válido.");
            return await _auth.RecuperarContrasenaAsync(correo);
        }
    }
}