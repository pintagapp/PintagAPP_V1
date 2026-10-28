using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Cuenta;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IniciarSesion _iniciarSesion;
        private readonly SesionActual _sesion;

        public LoginViewModel(IniciarSesion iniciarSesion, SesionActual sesion)
        {
            _iniciarSesion = iniciarSesion;
            _sesion = sesion;
        }

        [ObservableProperty] private string correo = "";
        [ObservableProperty] private string contrasena = "";
        [ObservableProperty] private bool estaOcupado;
        [ObservableProperty] private string mensaje = "";
        [ObservableProperty] private bool mensajeEsError;

        [RelayCommand]
        private async Task IniciarSesionAsync()
        {
            if (EstaOcupado) return;
            EstaOcupado = true;
            Mensaje = "";

            var resultado = await _iniciarSesion.EjecutarAsync(Correo, Contrasena);

            MensajeEsError = !resultado.EsExito;
            Mensaje = resultado.EsExito && _sesion.Usuario!.EsAdmin
                ? resultado.Mensaje + " (administrador)"
                : resultado.Mensaje;

            EstaOcupado = false;
        }

        [RelayCommand]
        private async Task IrARegistroAsync()
        {
            await Shell.Current.GoToAsync("RegistroPage");
        }
    }
}