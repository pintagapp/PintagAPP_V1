using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Ofertas;

namespace PintagAPP.Presentacion.ViewModels
{
    public record OfertaResumen(long Id, string Titulo, string Tipo, string Precio, string? FotoUrl, bool Activa)
    {
        public bool TieneFoto => !string.IsNullOrEmpty(FotoUrl);
        public string EstadoTexto => Activa ? "🟢 Activa" : "⏸️ Pausada";
    }

    public partial class MisOfertasViewModel : ObservableObject
    {
        private readonly GestionarMisOfertas _gestionar;

        public MisOfertasViewModel(GestionarMisOfertas gestionar)
        {
            _gestionar = gestionar;
        }

        public ObservableCollection<OfertaResumen> Ofertas { get; } = new();

        [ObservableProperty] private OfertaResumen? ofertaSeleccionada;
        [ObservableProperty] private bool puedeCrear;
        [ObservableProperty] private string aviso = "";
        [ObservableProperty] private bool tieneAviso;
        [ObservableProperty] private bool estaCargando;

        // Se recarga cada vez que la pantalla aparece (por ejemplo, al volver de editar)
        public async Task CargarAsync()
        {
            EstaCargando = true;
            Aviso = "";
            Ofertas.Clear();
            try
            {
                var perfil = await _gestionar.ObtenerMiPerfilAsync();
                if (perfil is null)
                {
                    MostrarAviso("Primero completa tu perfil en Mi negocio.");
                    PuedeCrear = false;
                    return;
                }
                if (perfil.Estado == "bloqueado")
                {
                    MostrarAviso("⛔ Tu perfil está bloqueado. Revisa el motivo en Mi negocio.");
                    PuedeCrear = false;
                    return;
                }

                var ofertas = await _gestionar.ObtenerOfertasAsync(perfil.Id);
                foreach (var o in ofertas)
                    Ofertas.Add(new OfertaResumen(o.Id, o.Titulo, o.EsProducto ? "Producto" : "Servicio",
                                                  o.TextoPrecio, o.Fotos.FirstOrDefault()?.Url, o.Activa));

                PuedeCrear = Ofertas.Count < GestionarMisOfertas.MaximoOfertas;

                if (perfil.Estado == "pendiente")
                    MostrarAviso("⏳ Tus ofertas se publicarán cuando el administrador apruebe tu perfil.");
                else if (Ofertas.Count == 0)
                    MostrarAviso("Aún no tienes ofertas. ¡Crea la primera!");
            }
            catch (Exception)
            {
                MostrarAviso("No se pudieron cargar tus ofertas. Revisa tu conexión.");
            }
            finally
            {
                TieneAviso = Aviso.Length > 0;
                EstaCargando = false;
            }
        }

        private void MostrarAviso(string texto) => Aviso = texto;

        [RelayCommand]
        private async Task NuevaAsync()
        {
            await Shell.Current.GoToAsync("OfertaFormPage?id=0");
        }

        // Al tocar una oferta de la lista, se abre para editarla
        async partial void OnOfertaSeleccionadaChanged(OfertaResumen? value)
        {
            if (value is null) return;
            OfertaSeleccionada = null;   // limpia la selección para poder volver a tocarla
            await Shell.Current.GoToAsync($"OfertaFormPage?id={value.Id}");
        }
    }
}