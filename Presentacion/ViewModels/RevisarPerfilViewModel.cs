using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Perfiles;
using PintagAPP.Dominio.Entidades;
using System.Collections.ObjectModel;
using static Android.Util.EventLogTags;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class RevisarPerfilViewModel : ObservableObject, IQueryAttributable
    {
        private readonly ModerarPerfiles _moderar;
        private long _perfilId;

        public RevisarPerfilViewModel(ModerarPerfiles moderar)
        {
            _moderar = moderar;
        }

        public ObservableCollection<Oferta> Ofertas { get; } = new();

        [ObservableProperty] private string nombre = "";
        [ObservableProperty] private string? negocio;
        [ObservableProperty] private bool tieneNegocio;
        [ObservableProperty] private string telefono = "";
        [ObservableProperty] private string barrio = "";
        [ObservableProperty] private string descripcion = "";
        [ObservableProperty] private string? fotoUrl;
        [ObservableProperty] private bool tieneFoto;
        [ObservableProperty] private string estadoTexto = "";
        [ObservableProperty] private string ofertasTexto = "";
        [ObservableProperty] private bool estaCargando;
        [ObservableProperty] private bool hayDatos;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AprobarCommand))]
        [NotifyCanExecuteChangedFor(nameof(BloquearCommand))]
        private bool estaOcupado;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _perfilId = query.TryGetValue("id", out var valor) && long.TryParse(valor?.ToString(), out var id) ? id : 0;
        }

        public async Task CargarAsync()
        {
            EstaCargando = true;
            HayDatos = false;
            Ofertas.Clear();
            try
            {
                var (perfil, ofertas) = await _moderar.ObtenerDetalleAsync(_perfilId);
                if (perfil is null)
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "No se encontró el perfil.", "Aceptar");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                Nombre = perfil.NombrePersona;
                Negocio = perfil.NombreNegocio;
                TieneNegocio = !string.IsNullOrEmpty(perfil.NombreNegocio);
                Telefono = perfil.Telefono;
                Barrio = perfil.BarrioNombre ?? "";
                Descripcion = perfil.Descripcion;
                FotoUrl = perfil.FotoUrl;
                TieneFoto = !string.IsNullOrEmpty(perfil.FotoUrl);
                EstadoTexto = perfil.Estado switch
                {
                    "aprobado" => "🟢 Aprobado",
                    "bloqueado" => "⛔ Bloqueado",
                    _ => "⏳ Pendiente de revisión"
                };

                foreach (var o in ofertas)
                    Ofertas.Add(o);
                OfertasTexto = ofertas.Count == 0
                    ? "Este perfil aún no tiene ofertas."
                    : $"Ofertas ({ofertas.Count})";

                HayDatos = true;
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlertAsync("Error", "No se pudo cargar el perfil. Revisa tu conexión.", "Aceptar");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private bool PuedeModerar() => !EstaOcupado;

        [RelayCommand(CanExecute = nameof(PuedeModerar))]
        private async Task AprobarAsync()
        {
            bool confirmar = await Shell.Current.DisplayAlertAsync("Aprobar perfil",
                $"¿Aprobar el perfil de {Nombre}? Sus datos y ofertas activas serán visibles para todos.",
                "Aprobar", "Cancelar");
            if (!confirmar) return;

            EstaOcupado = true;
            try
            {
                var resultado = await _moderar.AprobarAsync(_perfilId);
                await TerminarAsync(resultado.EsExito, resultado.Mensaje);
            }
            finally
            {
                EstaOcupado = false;
            }
        }

        [RelayCommand(CanExecute = nameof(PuedeModerar))]
        private async Task BloquearAsync()
        {
            var motivo = await Shell.Current.DisplayPromptAsync("Bloquear perfil",
                "Escribe el motivo. El oferente lo verá en Mi negocio.",
                "Bloquear", "Cancelar", "Ej.: La foto no corresponde a la persona",
                ModerarPerfiles.MaximoMotivo);
            if (motivo is null) return;   // canceló

            EstaOcupado = true;
            try
            {
                var resultado = await _moderar.BloquearAsync(_perfilId, motivo);
                await TerminarAsync(resultado.EsExito, resultado.Mensaje);
            }
            finally
            {
                EstaOcupado = false;
            }
        }

        // Muestra el mensaje y, si salió bien, vuelve a la lista (que se recarga sola)
        private async Task TerminarAsync(bool exito, string mensaje)
        {
            await Shell.Current.DisplayAlertAsync(exito ? "Listo" : "Error", mensaje, "Aceptar");
            if (exito)
                await Shell.Current.GoToAsync("..");
        }
    }
}