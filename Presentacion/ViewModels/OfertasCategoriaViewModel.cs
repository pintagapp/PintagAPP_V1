using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PintagAPP.CasosUso.Ofertas;

namespace PintagAPP.Presentacion.ViewModels
{
    public record OfertaPublicaResumen(long Id, string Titulo, string Precio, string Oferente, string Barrio,
                                    string? FotoUrl, string Calificacion)
    {
        public bool TieneFoto => !string.IsNullOrEmpty(FotoUrl);
    }

    public partial class OfertasCategoriaViewModel : ObservableObject, IQueryAttributable
    {
        private readonly ExplorarOfertas _explorar;
        private long _categoriaId;

        public OfertasCategoriaViewModel(ExplorarOfertas explorar)
        {
            _explorar = explorar;
        }

        public ObservableCollection<OfertaPublicaResumen> Ofertas { get; } = new();

        [ObservableProperty] private string nombreCategoria = "Ofertas";
        [ObservableProperty] private OfertaPublicaResumen? ofertaSeleccionada;
        [ObservableProperty] private string aviso = "";
        [ObservableProperty] private bool tieneAviso;
        [ObservableProperty] private bool estaCargando;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var id))
                _categoriaId = Convert.ToInt64(id);
            if (query.TryGetValue("nombre", out var nombre))
                NombreCategoria = nombre?.ToString() ?? "Ofertas";
        }

        public async Task CargarAsync()
        {
            EstaCargando = true;
            Aviso = "";
            Ofertas.Clear();
            try
            {
                var lista = await _explorar.ListarPorCategoriaAsync(_categoriaId);
                foreach (var o in lista)
                    Ofertas.Add(new OfertaPublicaResumen(o.Id, o.Titulo, o.TextoPrecio,
                                                  o.Oferente?.NombreVisible ?? "",
                                                  o.Oferente?.BarrioNombre ?? "",
                                                  o.FotoPrincipal,
                                                  o.Oferente?.TextoCalificacion ?? "Sin calificaciones"));

                if (Ofertas.Count == 0)
                    Aviso = "Aún no hay ofertas en esta categoría.";
            }
            catch (Exception)
            {
                Aviso = "No se pudieron cargar las ofertas. Revisa tu conexión.";
            }
            finally
            {
                TieneAviso = Aviso.Length > 0;
                EstaCargando = false;
            }
        }

        // Al tocar una oferta se abre su detalle
        async partial void OnOfertaSeleccionadaChanged(OfertaPublicaResumen? value)
        {
            if (value is null) return;
            OfertaSeleccionada = null;   // limpia la selección para poder volver a tocarla
            await Shell.Current.GoToAsync("DetalleOfertaPage",
                new Dictionary<string, object> { ["id"] = value.Id });
        }
    }
}
