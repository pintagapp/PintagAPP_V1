using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Ofertas;
using PintagAPP.Dominio.Entidades;
using System.Collections.ObjectModel;
using static Android.Icu.Text.CaseMap;
using static Android.Util.EventLogTags;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class DetalleOfertaViewModel : ObservableObject, IQueryAttributable
    {
        private readonly ExplorarOfertas _explorar;
        private long _ofertaId;
        private Oferta? _oferta;

        public DetalleOfertaViewModel(ExplorarOfertas explorar)
        {
            _explorar = explorar;
        }

        public ObservableCollection<FotoOferta> Fotos { get; } = new();

        [ObservableProperty] private string titulo = "";
        [ObservableProperty] private string precio = "";
        [ObservableProperty] private string tipoTexto = "";
        [ObservableProperty] private string descripcion = "";
        [ObservableProperty] private bool tieneFotos;
        [ObservableProperty] private bool sinFotos;

        [ObservableProperty] private string nombreOferente = "";
        [ObservableProperty] private string? fotoOferente;
        [ObservableProperty] private bool tieneFotoOferente;
        [ObservableProperty] private string barrio = "";
        [ObservableProperty] private string descripcionOferente = "";

        [ObservableProperty] private bool esMia;
        [ObservableProperty] private bool puedeContactar;
        [ObservableProperty] private bool hayDatos;
        [ObservableProperty] private bool estaCargando;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(WhatsAppCommand))]
        [NotifyCanExecuteChangedFor(nameof(LlamarCommand))]
        private bool estaOcupado;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var id))
                _ofertaId = Convert.ToInt64(id);
        }

        public async Task CargarAsync()
        {
            EstaCargando = true;
            HayDatos = false;
            Fotos.Clear();
            try
            {
                _oferta = await _explorar.ObtenerDetalleAsync(_ofertaId);
                if (_oferta is null)
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Esta oferta ya no está disponible.", "Aceptar");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                Titulo = _oferta.Titulo;
                Precio = _oferta.TextoPrecio;
                TipoTexto = _oferta.EsProducto ? "🛒 Producto" : "🛠️ Servicio";
                Descripcion = _oferta.Descripcion;
                foreach (var f in _oferta.Fotos)
                    Fotos.Add(f);
                TieneFotos = Fotos.Count > 0;
                SinFotos = !TieneFotos;

                var oferente = _oferta.Oferente;
                NombreOferente = oferente?.NombreVisible ?? "";
                FotoOferente = oferente?.FotoUrl;
                TieneFotoOferente = !string.IsNullOrEmpty(FotoOferente);
                Barrio = oferente?.BarrioNombre ?? "";
                DescripcionOferente = oferente?.Descripcion ?? "";

                EsMia = await _explorar.EsMiaAsync(_oferta);
                PuedeContactar = !EsMia;
                HayDatos = true;
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlertAsync("Error", "No se pudo cargar la oferta. Revisa tu conexión.", "Aceptar");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private bool PuedeTocar() => !EstaOcupado;

        [RelayCommand(CanExecute = nameof(PuedeTocar))]
        private Task WhatsAppAsync() => ContactarAsync(ExplorarOfertas.CanalWhatsApp);

        [RelayCommand(CanExecute = nameof(PuedeTocar))]
        private Task LlamarAsync() => ContactarAsync(ExplorarOfertas.CanalLlamada);

        private async Task ContactarAsync(string canal)
        {
            if (_oferta is null) return;

            if (!_explorar.HaySesion)
            {
                bool irALogin = await Shell.Current.DisplayAlertAsync("Inicia sesión",
                    "Para contactar al oferente necesitas una cuenta. Así cuidamos los datos de los emprendedores.",
                    "Iniciar sesión", "Ahora no");
                if (irALogin)
                    await Shell.Current.GoToAsync("LoginPage");
                return;
            }

            EstaOcupado = true;
            try
            {
                var (resultado, datos) = await _explorar.ContactarAsync(_oferta, canal);
                if (!resultado.EsExito || datos is null)
                {
                    await Shell.Current.DisplayAlertAsync("Error", resultado.Mensaje, "Aceptar");
                    return;
                }

                if (canal == ExplorarOfertas.CanalWhatsApp)
                {
                    var texto = Uri.EscapeDataString(ExplorarOfertas.MensajeWhatsApp(_oferta));
                    await Launcher.Default.OpenAsync(new Uri($"https://wa.me/{datos.NumeroWhatsApp}?text={texto}"));
                }
                else if (PhoneDialer.Default.IsSupported)
                {
                    PhoneDialer.Default.Open(datos.Telefono);
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Teléfono", $"Llama al {datos.Telefono}", "Aceptar");
                }
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlertAsync("Error", "No se pudo abrir WhatsApp o el marcador.", "Aceptar");
            }
            finally
            {
                EstaOcupado = false;
            }
        }
    }
}