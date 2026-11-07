using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Perfiles;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class MiPerfilViewModel : ObservableObject
    {
        private readonly SesionActual _sesion;
        private readonly ControlSesion _controlSesion;
        private readonly ModerarPerfiles _moderar;

        public MiPerfilViewModel(SesionActual sesion, ControlSesion controlSesion, ModerarPerfiles moderar)
        {
            _sesion = sesion;
            _controlSesion = controlSesion;
            _moderar = moderar;
        }

        [ObservableProperty] private bool conSesion;
        [ObservableProperty] private bool sinSesion = true;
        [ObservableProperty] private string nombre = "";
        [ObservableProperty] private bool esAdmin;
        [ObservableProperty] private string textoPanelAdmin = "🛡️ Panel de administración";

        public async Task ActualizarAsync()
        {
            ConSesion = _sesion.EstaIniciada;
            SinSesion = !_sesion.EstaIniciada;
            Nombre = _sesion.Usuario?.NombresCompletos ?? "";
            EsAdmin = _sesion.Usuario?.EsAdmin ?? false;

            if (!EsAdmin) return;

            // Muestra cuántos perfiles esperan revisión
            try
            {
                int pendientes = await _moderar.ContarPendientesAsync();
                TextoPanelAdmin = pendientes > 0
                    ? $"🛡️ Panel de administración ({pendientes} pendientes)"
                    : "🛡️ Panel de administración";
            }
            catch (Exception)
            {
                TextoPanelAdmin = "🛡️ Panel de administración";
            }
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
            await ActualizarAsync();
        }

        [RelayCommand]
        private async Task IrAMiNegocioAsync()
        {
            await Shell.Current.GoToAsync("OfrecerPage");
        }

        [RelayCommand]
        private async Task IrAMisOfertasAsync()
        {
            await Shell.Current.GoToAsync("MisOfertasPage");
        }

        [RelayCommand]
        private async Task IrAPanelAdminAsync()
        {
            await Shell.Current.GoToAsync("PanelAdminPage");
        }
    }
}