using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Ofertas;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class BuscarViewModel : ObservableObject
    {
        private readonly ExplorarOfertas _explorar;

        public BuscarViewModel(ExplorarOfertas explorar)
        {
            _explorar = explorar;
        }

        public ObservableCollection<OfertaPublicaResumen> Resultados { get; } = new();

        [ObservableProperty] private string texto = "";
        [ObservableProperty] private OfertaPublicaResumen? ofertaSeleccionada;
        [ObservableProperty] private string aviso = "Escribe lo que necesitas: pan, mecánico, guía, cuyes…";
        [ObservableProperty] private bool estaCargando;

        [RelayCommand]
        private async Task BuscarAsync()
        {
            var busqueda = Texto?.Trim() ?? "";
            if (busqueda.Length < ExplorarOfertas.MinimoBusqueda)
            {
                Aviso = $"Escribe al menos {ExplorarOfertas.MinimoBusqueda} letras.";
                return;
            }

            EstaCargando = true;
            Resultados.Clear();
            try
            {
                var lista = await _explorar.BuscarAsync(busqueda);
                foreach (var o in lista)
                    Resultados.Add(new OfertaPublicaResumen(o.Id, o.Titulo, o.TextoPrecio,
                                                            o.Oferente?.NombreVisible ?? "",
                                                            o.Oferente?.BarrioNombre ?? "",
                                                            o.FotoPrincipal,
                                                            o.Oferente?.TextoCalificacion ?? "Sin calificaciones"));

                Aviso = Resultados.Count == 0
                    ? $"No encontramos ofertas para «{busqueda}». Prueba con otra palabra."
                    : $"Resultados para «{busqueda}»: {Resultados.Count}";
            }
            catch (Exception)
            {
                Aviso = "No se pudo buscar. Revisa tu conexión.";
            }
            finally
            {
                EstaCargando = false;
            }
        }

        // Si borra el texto, se limpia la lista
        partial void OnTextoChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Resultados.Clear();
                Aviso = "Escribe lo que necesitas: pan, mecánico, guía, cuyes…";
            }
        }

        async partial void OnOfertaSeleccionadaChanged(OfertaPublicaResumen? value)
        {
            if (value is null) return;
            OfertaSeleccionada = null;
            await Shell.Current.GoToAsync("DetalleOfertaPage",
                new Dictionary<string, object> { ["id"] = value.Id });
        }
    }
}