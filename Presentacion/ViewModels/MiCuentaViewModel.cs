using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Cuenta;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class MiCuentaViewModel : ObservableObject
    {
        private readonly GestionarMiCuenta _cuenta;

        public MiCuentaViewModel(GestionarMiCuenta cuenta)
        {
            _cuenta = cuenta;
        }

        public string Nombre => _cuenta.Nombre;
        public string RolTexto => _cuenta.RolTexto;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EliminarCuentaCommand))]
        private bool estaOcupado;

        [RelayCommand]
        private async Task VerPoliticaAsync()
        {
            await Shell.Current.GoToAsync("PoliticaPrivacidadPage");
        }

        private bool PuedeEliminar() => !EstaOcupado;

        // Confirmación doble: aviso + escribir ELIMINAR
        [RelayCommand(CanExecute = nameof(PuedeEliminar))]
        private async Task EliminarCuentaAsync()
        {
            bool seguir = await Shell.Current.DisplayAlertAsync("Eliminar mi cuenta",
                "Se borrarán para siempre tu cuenta, tu negocio, tus ofertas, fotos, calificaciones y denuncias. " +
                "Esta acción no se puede deshacer.", "Continuar", "Cancelar");
            if (!seguir) return;

            var texto = await Shell.Current.DisplayPromptAsync("Confirmación",
                $"Escribe {GestionarMiCuenta.PalabraConfirmacion} para confirmar:",
                "Eliminar", "Cancelar", maxLength: 10);
            if (texto is null) return;

            EstaOcupado = true;
            try
            {
                var resultado = await _cuenta.EliminarCuentaAsync(texto);
                await Shell.Current.DisplayAlertAsync(resultado.EsExito ? "Cuenta eliminada" : "Aviso",
                                                      resultado.Mensaje, "Aceptar");
                if (resultado.EsExito)
                    await Shell.Current.GoToAsync("//InicioPage");
            }
            finally
            {
                EstaOcupado = false;
            }
        }
    }
}
