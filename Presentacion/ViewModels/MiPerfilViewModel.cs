using Android.Net.Sip;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using PintagAPP.CasosUso.Cuenta;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class MiPerfilViewModel : ObservableObject
    {
        private readonly SesionActual _sesion;
        private readonly ControlSesion _controlSesion;

        public MiPerfilViewModel(SesionActual sesion, ControlSesion controlSesion)
        {
            _sesion = sesion;
            _controlSesion = controlSesion;
        }

        [ObservableProperty] private bool conSesion;
        [ObservableProperty] private bool sinSesion = true;
        [ObservableProperty] private string nombre = "";
        [ObservableProperty] private bool esAdmin;

        public void Actualizar()
        {
            ConSesion = _sesion.EstaIniciada;
            SinSesion = !_sesion.EstaIniciada;
            Nombre = _sesion.Usuario?.NombresCompletos ?? "";
            EsAdmin = _sesion.Usuario?.EsAdmin ?? false;
        }

        [RelayCommand]
        private async Task IrALoginAsync()
        {
            await Shell.Current.GoToAsync("LoginPage");
        }

        [RelayCommand]
        private async Task CerrarSesionAsync()
        {
            await _controlSesion.CerrarAsync();
            Actualizar();
        }
        [RelayCommand]
        private async Task IrAMiNegocioAsync()
        {
            await Shell.Current.GoToAsync("OfrecerPage");
        }
    }
}
