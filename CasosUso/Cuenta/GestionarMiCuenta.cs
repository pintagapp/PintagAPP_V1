using PintagAPP.CasosUso.Interfaces;

namespace PintagAPP.CasosUso.Cuenta
{
    public class GestionarMiCuenta
    {
        public const string PalabraConfirmacion = "ELIMINAR";

        private readonly IAuthServicio _auth;
        private readonly IAlmacenamientoServicio _almacenamiento;
        private readonly SesionActual _sesion;

        public GestionarMiCuenta(IAuthServicio auth, IAlmacenamientoServicio almacenamiento, SesionActual sesion)
        {
            _auth = auth;
            _almacenamiento = almacenamiento;
            _sesion = sesion;
        }

        public string Nombre => _sesion.Usuario?.NombresCompletos ?? "";
        public string RolTexto => (_sesion.Usuario?.EsAdmin ?? false) ? "Administrador" : "Usuario";

        public async Task<Resultado> EliminarCuentaAsync(string? confirmacion)
        {
            if (!_sesion.EstaIniciada)
                return Resultado.Error("No has iniciado sesión.");
            if (confirmacion?.Trim().ToUpperInvariant() != PalabraConfirmacion)
                return Resultado.Error($"Para confirmar debes escribir {PalabraConfirmacion}.");

            // 1) Primero las fotos (el bucket no se borra en cascada)
            try
            {
                await _almacenamiento.EliminarCarpetaAsync(_sesion.Usuario!.Id.ToString());
            }
            catch (Exception)
            {
                // Si falla, igual se elimina la cuenta; las fotos quedan sin enlace
            }

            // 2) Luego la cuenta y todos sus registros
            return await _auth.EliminarCuentaAsync();
        }
    }
}
