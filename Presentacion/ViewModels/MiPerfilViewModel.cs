using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Calificaciones;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Denuncias;
using PintagAPP.CasosUso.Perfiles;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class MiPerfilViewModel : ObservableObject
    {
        private readonly SesionActual _sesion;
        private readonly ControlSesion _controlSesion;
        private readonly ModerarPerfiles _moderar;
        private readonly CalificarOferentes _calificar;
        private readonly GestionarDenuncias _denuncias;

        public MiPerfilViewModel(SesionActual sesion, ControlSesion controlSesion,
                                 ModerarPerfiles moderar, CalificarOferentes calificar,
                                 GestionarDenuncias denuncias)
        {
            _sesion = sesion;
            _controlSesion = controlSesion;
            _moderar = moderar;
            _calificar = calificar;
            _denuncias = denuncias;
        }

        [ObservableProperty] private bool conSesion;
        [ObservableProperty] private bool sinSesion = true;
        [ObservableProperty] private string nombre = "";
        [ObservableProperty] private bool esAdmin;
        [ObservableProperty] private string textoPanelAdmin = "🛡️ Panel de administración";
        [ObservableProperty] private string textoPorCalificar = "⭐ Por calificar";
        [ObservableProperty] private string textoDenuncias = "🚩 Denuncias";

        public async Task ActualizarAsync()
        {
            ConSesion = _sesion.EstaIniciada;
            SinSesion = !_sesion.EstaIniciada;
            Nombre = _sesion.Usuario?.NombresCompletos ?? "";
            EsAdmin = _sesion.Usuario?.EsAdmin ?? false;

            if (!ConSesion) return;

            // Recordatorio: cuántos oferentes contactó y aún no calificó
            try
            {
                int porCalificar = (await _calificar.ObtenerPorCalificarAsync()).Count;
                TextoPorCalificar = porCalificar > 0
                    ? $"⭐ Por calificar ({porCalificar})"
                    : "⭐ Por calificar";
            }
            catch (Exception)
            {
                TextoPorCalificar = "⭐ Por calificar";
            }

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

            // Muestra cuántas denuncias hay pendientes
            try
            {
                int denunciasPendientes = (await _denuncias.ObtenerPendientesAsync()).Count;
                TextoDenuncias = denunciasPendientes > 0
                    ? $"🚩 Denuncias ({denunciasPendientes} pendientes)"
                    : "🚩 Denuncias";
            }
            catch (Exception)
            {
                TextoDenuncias = "🚩 Denuncias";
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
        private async Task IrAPorCalificarAsync()
        {
            await Shell.Current.GoToAsync("PorCalificarPage");
        }

        [RelayCommand]
        private async Task IrAPanelAdminAsync()
        {
            await Shell.Current.GoToAsync("PanelAdminPage");
        }

        [RelayCommand]
        private async Task IrADenunciasAsync()
        {
            await Shell.Current.GoToAsync("DenunciasPage");
        }

        [RelayCommand]
        private async Task IrAMiCuentaAsync()
        {
            await Shell.Current.GoToAsync("MiCuentaPage");
        }

        [RelayCommand]
        private async Task IrAEstadisticasAsync()
        {
            await Shell.Current.GoToAsync("EstadisticasPage");
        }

        [RelayCommand]
        private async Task IrANoticiasAdminAsync()
        {
            await Shell.Current.GoToAsync("AdminNoticiasPage");
        }
    }
}