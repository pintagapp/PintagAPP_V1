using PintagAPP.CasosUso.Interfaces;

namespace PintagAPP.CasosUso.Cuenta
{
    public class ControlSesion
    {
        private readonly IAuthServicio _auth;

        public ControlSesion(IAuthServicio auth)
        {
            _auth = auth;
        }

        public Task<bool> RestaurarAsync() => _auth.RestaurarSesionAsync();

        public Task CerrarAsync() => _auth.CerrarSesionAsync();
    }
}