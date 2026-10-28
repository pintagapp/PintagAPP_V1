using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Perfiles;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class OfrecerViewModel : ObservableObject
    {
        private readonly GestionarMiPerfil _gestionar;
        private readonly SesionActual _sesion;
        private PerfilOferente _perfil = new();
        private byte[]? _fotoNueva;
        private bool _cargado;

        public OfrecerViewModel(GestionarMiPerfil gestionar, SesionActual sesion)
        {
            _gestionar = gestionar;
            _sesion = sesion;
        }

        public ObservableCollection<Barrio> Barrios { get; } = new();

        [ObservableProperty] private string nombrePersona = "";
        [ObservableProperty] private string nombreNegocio = "";
        [ObservableProperty] private string telefono = "";
        [ObservableProperty] private string descripcion = "";
        [ObservableProperty] private Barrio? barrioSeleccionado;
        [ObservableProperty] private ImageSource? foto;
        [ObservableProperty] private bool esNuevo = true;
        [ObservableProperty] private bool autorizaPublicacion;
        [ObservableProperty] private string textoBoton = "Enviar a revisión";
        [ObservableProperty] private string estadoTexto = "";
        [ObservableProperty] private bool tieneEstado;
        [ObservableProperty] private bool estaOcupado;
        [ObservableProperty] private string mensaje = "";
        [ObservableProperty] private bool mensajeEsError;

        public async Task CargarAsync()
        {
            if (_cargado) return;
            EstaOcupado = true;
            try
            {
                foreach (var b in await _gestionar.ObtenerBarriosAsync())
                    Barrios.Add(b);

                var perfil = await _gestionar.ObtenerAsync();
                if (perfil is null)
                {
                    NombrePersona = _sesion.Usuario?.NombresCompletos ?? "";
                }
                else
                {
                    _perfil = perfil;
                    NombrePersona = perfil.NombrePersona;
                    NombreNegocio = perfil.NombreNegocio ?? "";
                    Telefono = perfil.Telefono;
                    Descripcion = perfil.Descripcion;
                    BarrioSeleccionado = Barrios.FirstOrDefault(b => b.Id == perfil.BarrioId);
                    if (!string.IsNullOrEmpty(perfil.FotoUrl))
                        Foto = ImageSource.FromUri(new Uri(perfil.FotoUrl));
                }
                ActualizarEstado();
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

        // ---------- FOTO CON CÁMARA ----------
        [RelayCommand]
        private async Task TomarFotoAsync()
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    MensajeEsError = true;
                    Mensaje = "Este dispositivo no tiene cámara disponible.";
                    return;
                }

                var permiso = await Permissions.RequestAsync<Permissions.Camera>();
                if (permiso != PermissionStatus.Granted)
                {
                    MensajeEsError = true;
                    Mensaje = "Necesitamos permiso de cámara para tomar la foto.";
                    return;
                }

                var archivo = await MediaPicker.Default.CapturePhotoAsync(OpcionesFoto());
                await ProcesarFotoAsync(archivo);
            }
            catch (Exception ex)
            {
                MensajeEsError = true;
                Mensaje = $"No se pudo tomar la foto: {ex.Message}";
            }
        }

        // ---------- FOTO DE LA GALERÍA ----------
        [RelayCommand]
        private async Task ElegirFotoAsync()
        {
            try
            {
                var archivo = await MediaPicker.Default.PickPhotoAsync(OpcionesFoto());
                await ProcesarFotoAsync(archivo);
            }
            catch (Exception ex)
            {
                MensajeEsError = true;
                Mensaje = $"No se pudo elegir la foto: {ex.Message}";
            }
        }

        // Reduce la foto en el celular antes de subirla
        private static MediaPickerOptions OpcionesFoto() => new()
        {
            Title = "Foto de perfil",
            MaximumWidth = 1024,
            MaximumHeight = 1024,
            CompressionQuality = 80
        };

        private async Task ProcesarFotoAsync(FileResult? archivo)
        {
            if (archivo is null) return;   // el usuario canceló

            using var stream = await archivo.OpenReadAsync();
            using var memoria = new MemoryStream();
            await stream.CopyToAsync(memoria);
            _fotoNueva = memoria.ToArray();

            var bytes = _fotoNueva;
            Foto = ImageSource.FromStream(() => new MemoryStream(bytes));
            Mensaje = "";
        }

        // ---------- GUARDAR ----------
        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (EstaOcupado) return;
            EstaOcupado = true;
            Mensaje = "";

            _perfil.NombreNegocio = NombreNegocio;
            _perfil.Telefono = Telefono;
            _perfil.Descripcion = Descripcion;
            _perfil.BarrioId = BarrioSeleccionado?.Id ?? 0;

            var resultado = await _gestionar.GuardarAsync(_perfil, _fotoNueva, AutorizaPublicacion);

            MensajeEsError = !resultado.EsExito;
            Mensaje = resultado.Mensaje;
            if (resultado.EsExito)
            {
                _fotoNueva = null;
                ActualizarEstado();
            }
            EstaOcupado = false;
        }

        private void ActualizarEstado()
        {
            EsNuevo = _perfil.EsNuevo;
            TextoBoton = EsNuevo ? "Enviar a revisión" : "Guardar cambios";
            TieneEstado = !EsNuevo;
            EstadoTexto = _perfil.Estado switch
            {
                "pendiente" => "⏳ En revisión: el administrador está revisando tu perfil.",
                "aprobado" => "✅ Aprobado: tu perfil es visible para la comunidad.",
                "bloqueado" => $"⛔ Bloqueado: {_perfil.MotivoEstado}",
                _ => ""
            };
        }
    }
}