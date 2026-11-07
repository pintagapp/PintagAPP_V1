using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Perfiles;

namespace PintagAPP.Presentacion.ViewModels
{
    public record PerfilPendienteResumen(long Id, string Nombre, string? Negocio, string Barrio,
                                         string Telefono, string? FotoUrl, DateTimeOffset CreadoEn)
    {
        public bool TieneFoto => !string.IsNullOrEmpty(FotoUrl);
        public bool TieneNegocio => !string.IsNullOrEmpty(Negocio);
        public string FechaTexto => "Enviado: " + CreadoEn.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    }

    public partial class PanelAdminViewModel : ObservableObject
    {
        private readonly ModerarPerfiles _moderar;

        public PanelAdminViewModel(ModerarPerfiles moderar)
        {
            _moderar = moderar;
        }

        public ObservableCollection<PerfilPendienteResumen> Pendientes { get; } = new();

        [ObservableProperty] private PerfilPendienteResumen? perfilSeleccionado;
        [ObservableProperty] private string aviso = "";
        [ObservableProperty] private bool tieneAviso;
        [ObservableProperty] private bool estaCargando;

        // Se recarga cada vez que la pantalla aparece (por ejemplo, al volver de aprobar)
        public async Task CargarAsync()
        {
            EstaCargando = true;
            Aviso = "";
            Pendientes.Clear();
            try
            {
                if (!_moderar.EsAdmin)
                {
                    Aviso = "Esta sección es solo para el administrador.";
                    return;
                }

                var perfiles = await _moderar.ObtenerPendientesAsync();
                foreach (var p in perfiles)
                    Pendientes.Add(new PerfilPendienteResumen(p.Id, p.NombrePersona, p.NombreNegocio,
                                                              p.BarrioNombre ?? "", p.Telefono, p.FotoUrl, p.CreadoEn));

                Aviso = Pendientes.Count == 0
                    ? "✅ No hay perfiles pendientes de revisión."
                    : $"Perfiles por revisar: {Pendientes.Count}. Toca uno para ver sus datos y ofertas.";
            }
            catch (Exception)
            {
                Aviso = "No se pudieron cargar los perfiles. Revisa tu conexión.";
            }
            finally
            {
                TieneAviso = Aviso.Length > 0;
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private Task RecargarAsync() => CargarAsync();

        // Al tocar un perfil se abre su revisión
        async partial void OnPerfilSeleccionadoChanged(PerfilPendienteResumen? value)
        {
            if (value is null) return;
            PerfilSeleccionado = null;   // limpia la selección para poder volver a tocarlo
            await Shell.Current.GoToAsync($"RevisarPerfilPage?id={value.Id}");
        }
    }
}
