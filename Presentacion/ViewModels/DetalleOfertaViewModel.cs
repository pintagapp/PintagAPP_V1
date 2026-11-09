using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Calificaciones;
using PintagAPP.CasosUso.Ofertas;
using PintagAPP.Dominio.Entidades;
using PintagAPP.CasosUso.Denuncias;

namespace PintagAPP.Presentacion.ViewModels
{
    public record OpinionItem(long Id, string Estrellas, string? Comentario, string Fecha)
    {
        public bool TieneComentario => !string.IsNullOrEmpty(Comentario);
    }

    public partial class DetalleOfertaViewModel : ObservableObject, IQueryAttributable
    {
        private readonly ExplorarOfertas _explorar;
        private readonly CalificarOferentes _calificar;
        private long _ofertaId;
        private Oferta? _oferta;
        private bool _preguntarAlVolver;   // se activa al abrir WhatsApp o el marcador

        private readonly GestionarDenuncias _denuncias;

        public DetalleOfertaViewModel(ExplorarOfertas explorar, CalificarOferentes calificar,
                                      GestionarDenuncias denuncias)
        {
            _explorar = explorar;
            _calificar = calificar;
            _denuncias = denuncias;
        }

        public ObservableCollection<FotoOferta> Fotos { get; } = new();
        public ObservableCollection<OpinionItem> Opiniones { get; } = new();

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
        [ObservableProperty] private string calificacionTexto = "Sin calificaciones";
        [ObservableProperty] private bool sinOpiniones;
        [ObservableProperty] private string textoBotonCalificar = "⭐ Calificar";

        [ObservableProperty] private bool esMia;
        [ObservableProperty] private bool puedeContactar;
        [ObservableProperty] private bool hayDatos;
        [ObservableProperty] private bool estaCargando;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(WhatsAppCommand))]
        [NotifyCanExecuteChangedFor(nameof(LlamarCommand))]
        [NotifyCanExecuteChangedFor(nameof(CalificarCommand))]
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
                CalificacionTexto = oferente?.TextoCalificacion ?? "Sin calificaciones";
                TextoBotonCalificar = $"⭐ Calificar a {NombreOferente}";

                EsMia = await _explorar.EsMiaAsync(_oferta);
                PuedeContactar = !EsMia;
                await CargarOpinionesAsync();
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

        // Al volver a la pantalla (después de calificar o de iniciar sesión): actualiza sin recargar fotos
        public async Task RefrescarAsync()
        {
            if (_oferta is null) return;
            try
            {
                var actual = await _explorar.ObtenerDetalleAsync(_ofertaId);
                if (actual?.Oferente is not null)
                    CalificacionTexto = actual.Oferente.TextoCalificacion;

                EsMia = await _explorar.EsMiaAsync(_oferta);
                PuedeContactar = !EsMia;
                await CargarOpinionesAsync();
            }
            catch (Exception)
            {
                // Si falla, se queda con lo que ya mostraba
            }
        }

        private async Task CargarOpinionesAsync()
        {
            if (_oferta is null) return;
            Opiniones.Clear();
            var lista = await _calificar.ObtenerOpinionesAsync(_oferta.PerfilId);
            foreach (var c in lista)
                Opiniones.Add(new OpinionItem(c.Id, c.TextoEstrellas, c.Comentario,
                              c.Fecha.ToLocalTime().ToString("dd/MM/yyyy")));
            SinOpiniones = Opiniones.Count == 0;
        }

        private bool PuedeTocar() => !EstaOcupado;

        [RelayCommand(CanExecute = nameof(PuedeTocar))]
        private Task WhatsAppAsync() => ContactarAsync(ExplorarOfertas.CanalWhatsApp);

        [RelayCommand(CanExecute = nameof(PuedeTocar))]
        private Task LlamarAsync() => ContactarAsync(ExplorarOfertas.CanalLlamada);

        [RelayCommand(CanExecute = nameof(PuedeTocar))]
        private async Task CalificarAsync()
        {
            if (_oferta is null) return;

            if (!_calificar.HaySesion)
            {
                await PedirSesionAsync("Para calificar al oferente necesitas una cuenta.");
                return;
            }

            EstaOcupado = true;
            try
            {
                var resultado = await _calificar.VerificarPuedeCalificarAsync(_oferta.PerfilId);
                if (!resultado.EsExito)
                {
                    await Shell.Current.DisplayAlertAsync("Calificar", resultado.Mensaje, "Aceptar");
                    return;
                }
                await IrACalificarAsync();
            }
            finally
            {
                EstaOcupado = false;
            }
        }

        // Recordatorio: al regresar de WhatsApp o de la llamada, pregunta una sola vez
        public async Task AlVolverALaAppAsync()
        {
            if (!_preguntarAlVolver || _oferta is null) return;
            _preguntarAlVolver = false;

            try
            {
                var yaCalifico = await _calificar.ObtenerMiCalificacionAsync(_oferta.PerfilId);
                if (yaCalifico is not null) return;

                bool calificar = await Shell.Current.DisplayAlertAsync($"¿Ya te atendió {NombreOferente}?",
                    "Tu opinión ayuda a otros vecinos de Píntag. ¿Quieres calificarlo?",
                    "Calificar", "Más tarde");
                if (calificar)
                    await IrACalificarAsync();
            }
            catch (Exception)
            {
                // El recordatorio es opcional: si falla, no se molesta al usuario
            }
        }
        // ---------- DENUNCIAS ----------
        [RelayCommand]
        private async Task DenunciarOfertaAsync()
        {
            if (_oferta is null) return;
            if (!_denuncias.HaySesion)
            {
                await PedirSesionAsync("Para denunciar necesitas una cuenta.");
                return;
            }

            var motivo = await PedirMotivoAsync("¿Por qué denuncias esta oferta?");
            if (motivo is null) return;

            var resultado = await _denuncias.DenunciarOfertaAsync(_oferta.Id, motivo);
            await Shell.Current.DisplayAlertAsync(resultado.EsExito ? "Denuncia enviada" : "Error",
                                                  resultado.Mensaje, "Aceptar");
        }

        [RelayCommand]
        private async Task DenunciarOpinionAsync(long calificacionId)
        {
            if (!_denuncias.HaySesion)
            {
                await PedirSesionAsync("Para denunciar necesitas una cuenta.");
                return;
            }

            var motivo = await PedirMotivoAsync("¿Por qué denuncias esta opinión?");
            if (motivo is null) return;

            var resultado = await _denuncias.DenunciarOpinionAsync(calificacionId, motivo);
            await Shell.Current.DisplayAlertAsync(resultado.EsExito ? "Denuncia enviada" : "Error",
                                                  resultado.Mensaje, "Aceptar");
        }

        // Lista de motivos; "Otro" pide escribirlo
        private static async Task<string?> PedirMotivoAsync(string titulo)
        {
            var opcion = await Shell.Current.DisplayActionSheetAsync(titulo, "Cancelar", null,
                                                                     GestionarDenuncias.Motivos);
            if (opcion is null || opcion == "Cancelar") return null;
            if (opcion != "Otro") return opcion;

            var texto = await Shell.Current.DisplayPromptAsync("Otro motivo", "Cuéntanos qué pasa:",
                                                               "Enviar", "Cancelar", maxLength: 280);
            return string.IsNullOrWhiteSpace(texto) ? null : $"Otro: {texto.Trim()}";
        }
        private Task IrACalificarAsync() =>
            Shell.Current.GoToAsync("CalificarPage", new Dictionary<string, object>
            {
                ["perfilId"] = _oferta!.PerfilId,
                ["nombre"] = NombreOferente
            });

        private async Task PedirSesionAsync(string mensaje)
        {
            bool irALogin = await Shell.Current.DisplayAlertAsync("Inicia sesión",
                $"{mensaje} Así cuidamos los datos de los emprendedores.",
                "Iniciar sesión", "Ahora no");
            if (irALogin)
                await Shell.Current.GoToAsync("LoginPage");
        }

        private async Task ContactarAsync(string canal)
        {
            if (_oferta is null) return;

            if (!_explorar.HaySesion)
            {
                await PedirSesionAsync("Para contactar al oferente necesitas una cuenta.");
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
                    _preguntarAlVolver = true;
                }
                else if (PhoneDialer.Default.IsSupported)
                {
                    PhoneDialer.Default.Open(datos.Telefono);
                    _preguntarAlVolver = true;
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