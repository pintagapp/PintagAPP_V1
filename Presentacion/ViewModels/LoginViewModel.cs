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

            EstaOcupado = false;

            if (resultado.EsExito)
            {
                await Shell.Current.GoToAsync("..");   // vuelve a la pantalla anterior
                return;
            }

            MensajeEsError = true;
            Mensaje = resultado.Mensaje;
        }

        // Pide el correo y Supabase envía el enlace para crear una nueva contraseña
        [RelayCommand]
        private async Task OlvideContrasenaAsync()
        {
            if (EstaOcupado) return;

            var correo = await Shell.Current.DisplayPromptAsync("Recuperar contraseña",
                "Escribe el correo de tu cuenta y te enviaremos un enlace para crear una nueva contraseña.",
                "Enviar", "Cancelar", placeholder: "correo@ejemplo.com",
                keyboard: Keyboard.Email, initialValue: Correo);

            if (string.IsNullOrWhiteSpace(correo)) return;   // tocó Cancelar

            EstaOcupado = true;
            var resultado = await _iniciarSesion.RecuperarContrasenaAsync(correo);
            EstaOcupado = false;

            await Shell.Current.DisplayAlertAsync(resultado.EsExito ? "Revisa tu correo" : "Aviso",
                                                  resultado.Mensaje, "Aceptar");
        }

        [RelayCommand]
        private async Task IrARegistroAsync()
        {
            await Shell.Current.GoToAsync("RegistroPage");
        }
    }
}