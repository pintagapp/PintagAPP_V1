using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Ofertas;
using PintagAPP.Dominio.Entidades;
using PintagAPP.Presentacion.Utilidades;

namespace PintagAPP.Presentacion.ViewModels
{
    // Una foto en pantalla: ya guardada (Existente) o recién tomada (Nueva)
    public class FotoItem
    {
        public FotoOferta? Existente { get; init; }
        public byte[]? Nueva { get; init; }
        public ImageSource Imagen { get; init; } = default!;
    }

    public partial class OfertaFormViewModel : ObservableObject, IQueryAttributable
    {
        private readonly GestionarMisOfertas _gestionar;
        private readonly List<FotoOferta> _fotosAEliminar = new();
        private Oferta _oferta = new();
        private long _ofertaId;
        private bool _cargado;

        public OfertaFormViewModel(GestionarMisOfertas gestionar)
        {
            _gestionar = gestionar;
        }

        public ObservableCollection<Categoria> Categorias { get; } = new();
        public ObservableCollection<FotoItem> Fotos { get; } = new();
        public List<string> Tipos { get; } = new() { "Servicio", "Producto" };

        [ObservableProperty] private string tipoSeleccionado = "Servicio";
        [ObservableProperty] private Categoria? categoriaSeleccionada;
        [ObservableProperty] private string titulo = "";
        [ObservableProperty] private string descripcion = "";
        [ObservableProperty] private string precioTexto = "";
        [ObservableProperty] private string unidadesTexto = "";
        [ObservableProperty] private bool aConvenir;
        [ObservableProperty] private bool activa = true;
        [ObservableProperty] private bool muestraPrecio = true;
        [ObservableProperty] private bool muestraUnidades;
        [ObservableProperty] private bool puedeAgregarFoto = true;
        [ObservableProperty] private bool esEdicion;
        [ObservableProperty] private FotoItem? fotoSeleccionada;
        [ObservableProperty] private bool estaOcupado;
        [ObservableProperty] private string mensaje = "";
        [ObservableProperty] private bool mensajeEsError;

        // Recibe "?id=..." de la navegación
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _ofertaId = query.TryGetValue("id", out var valor) && long.TryParse(valor?.ToString(), out var id) ? id : 0;
        }

        partial void OnTipoSeleccionadoChanged(string value) => ActualizarVisibilidad();
        partial void OnAConvenirChanged(bool value) => ActualizarVisibilidad();

        private void ActualizarVisibilidad()
        {
            MuestraPrecio = !AConvenir;
            MuestraUnidades = TipoSeleccionado == "Producto" && !AConvenir;
        }

        private void ActualizarFotos() => PuedeAgregarFoto = Fotos.Count < GestionarMisOfertas.MaximoFotos;

        public async Task CargarAsync()
        {
            if (_cargado) return;
            EstaOcupado = true;
            try
            {
                foreach (var c in await _gestionar.ObtenerCategoriasAsync())
                    Categorias.Add(c);

                if (_ofertaId > 0)
                {
                    var perfil = await _gestionar.ObtenerMiPerfilAsync();
                    var ofertas = perfil is null ? new List<Oferta>() : await _gestionar.ObtenerOfertasAsync(perfil.Id);
                    var o = ofertas.FirstOrDefault(x => x.Id == _ofertaId);
                    if (o is not null)
                    {
                        _oferta = o;
                        TipoSeleccionado = o.EsProducto ? "Producto" : "Servicio";
                        CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Id == o.CategoriaId);
                        Titulo = o.Titulo;
                        Descripcion = o.Descripcion;
                        AConvenir = o.AConvenir;
                        PrecioTexto = o.Precio?.ToString("0.00", CultureInfo.InvariantCulture) ?? "";
                        UnidadesTexto = o.UnidadesPorPrecio?.ToString() ?? "";
                        Activa = o.Activa;
                        foreach (var f in o.Fotos)
                            Fotos.Add(new FotoItem { Existente = f, Imagen = ImageSource.FromUri(new Uri(f.Url)) });
                    }
                }

                EsEdicion = !_oferta.EsNueva;
                ActualizarVisibilidad();
                ActualizarFotos();
                _cargado = true;
            }
            catch (Exception)
            {
                MensajeEsError = true;
                Mensaje = "No se pudo cargar la información. Revisa tu conexión.";
            }
            finally
            {
                EstaOcupado = false;
            }
        }

        // ---------- FOTOS ----------
        [RelayCommand]
        private async Task TomarFotoAsync() => await AgregarFotoAsync(() => SelectorFotos.TomarAsync("Foto de la oferta"));

        [RelayCommand]
        private async Task ElegirFotoAsync() => await AgregarFotoAsync(() => SelectorFotos.ElegirAsync("Foto de la oferta"));

        private async Task AgregarFotoAsync(Func<Task<byte[]?>> obtener)
        {
            try
            {
                if (!PuedeAgregarFoto) return;
                var bytes = await obtener();
                if (bytes is null) return;   // canceló
                Fotos.Add(new FotoItem { Nueva = bytes, Imagen = ImageSource.FromStream(() => new MemoryStream(bytes)) });
                ActualizarFotos();
                Mensaje = "";
            }
            catch (Exception ex)
            {
                MensajeEsError = true;
                Mensaje = ex.Message;
            }
        }

        // Al tocar una foto, pregunta si quitarla
        async partial void OnFotoSeleccionadaChanged(FotoItem? value)
        {
            if (value is null) return;
            FotoSeleccionada = null;

            bool quitar = await Shell.Current.DisplayAlertAsync("Quitar foto", "¿Quieres quitar esta foto?", "Quitar", "Cancelar");
            if (!quitar) return;

            if (value.Existente is not null)
                _fotosAEliminar.Add(value.Existente);
            Fotos.Remove(value);
            ActualizarFotos();
        }

        // ---------- GUARDAR ----------
        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (EstaOcupado) return;
            Mensaje = "";

            decimal? precio = null;
            if (!AConvenir)
            {
                if (!decimal.TryParse((PrecioTexto ?? "").Replace(',', '.'), NumberStyles.Number,
                                      CultureInfo.InvariantCulture, out var p))
                {
                    MensajeEsError = true;
                    Mensaje = "Escribe un precio válido, por ejemplo 1.50";
                    return;
                }
                precio = p;
            }

            int? unidades = null;
            if (MuestraUnidades && !string.IsNullOrWhiteSpace(UnidadesTexto))
            {
                if (!int.TryParse(UnidadesTexto, out var u))
                {
                    MensajeEsError = true;
                    Mensaje = "Las unidades deben ser un número entero, por ejemplo 8.";
                    return;
                }
                unidades = u;
            }

            _oferta.Tipo = TipoSeleccionado == "Producto" ? "producto" : "servicio";
            _oferta.CategoriaId = CategoriaSeleccionada?.Id ?? 0;
            _oferta.Titulo = Titulo;
            _oferta.Descripcion = Descripcion;
            _oferta.Precio = precio;
            _oferta.AConvenir = AConvenir;
            _oferta.UnidadesPorPrecio = unidades;
            _oferta.Activa = Activa;

            var nuevas = Fotos.Where(f => f.Nueva is not null).Select(f => f.Nueva!).ToList();

            EstaOcupado = true;
            var resultado = await _gestionar.GuardarAsync(_oferta, _fotosAEliminar, nuevas);
            EstaOcupado = false;

            if (resultado.EsExito)
            {
                await Shell.Current.DisplayAlertAsync("Listo", resultado.Mensaje, "Aceptar");
                await Shell.Current.GoToAsync("..");   // vuelve a Mis ofertas
                return;
            }

            MensajeEsError = true;
            Mensaje = resultado.Mensaje;
        }

        // ---------- ELIMINAR ----------
        [RelayCommand]
        private async Task EliminarAsync()
        {
            bool confirmar = await Shell.Current.DisplayAlertAsync("Eliminar oferta",
                "¿Seguro que quieres eliminar esta oferta? Esta acción no se puede deshacer.", "Eliminar", "Cancelar");
            if (!confirmar) return;

            EstaOcupado = true;
            var resultado = await _gestionar.EliminarAsync(_oferta);
            EstaOcupado = false;

            if (resultado.EsExito)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            MensajeEsError = true;
            Mensaje = resultado.Mensaje;
        }
    }
}
