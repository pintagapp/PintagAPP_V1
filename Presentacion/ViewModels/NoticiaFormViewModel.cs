using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Noticias;
using PintagAPP.Dominio.Entidades;
using PintagAPP.Presentacion.Utilidades;
using static Android.Icu.Text.CaseMap;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class NoticiaFormViewModel : ObservableObject, IQueryAttributable
    {
        private readonly AdministrarNoticias _admin;
        private Noticia _noticia = new();
        private byte[]? _nuevaImagen;

        public NoticiaFormViewModel(AdministrarNoticias admin)
        {
            _admin = admin;
        }

        [ObservableProperty] private string tituloPagina = "Nueva noticia";
        [ObservableProperty] private string titulo = "";
        [ObservableProperty] private string contenido = "";
        [ObservableProperty] private bool activa = true;
        [ObservableProperty] private ImageSource? vistaPrevia;
        [ObservableProperty] private bool tieneImagen;
        [ObservableProperty] private bool esEdicion;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
        [NotifyCanExecuteChangedFor(nameof(EliminarCommand))]
        private bool estaOcupado;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("noticia", out var valor) && valor is Noticia n)
            {
                _noticia = n;
                TituloPagina = "Editar noticia";
                EsEdicion = true;
                Titulo = n.Titulo;
                Contenido = n.Contenido;
                Activa = n.Activa;
                if (!string.IsNullOrEmpty(n.ImagenUrl))
                {
                    VistaPrevia = ImageSource.FromUri(new Uri(n.ImagenUrl));
                    TieneImagen = true;
                }
            }
        }

        [RelayCommand]
        private async Task ElegirImagenAsync()
        {
            try
            {
                var bytes = await SelectorFotos.ElegirAsync("Imagen de la noticia");
                if (bytes is null) return;
                _nuevaImagen = bytes;
                VistaPrevia = ImageSource.FromStream(() => new MemoryStream(bytes));
                TieneImagen = true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Aviso", ex.Message, "Aceptar");
            }
        }

        [RelayCommand]
        private void QuitarImagen()
        {
            _nuevaImagen = null;
            _noticia.ImagenUrl = null;
            VistaPrevia = null;
            TieneImagen = false;
        }

        private bool PuedeTocar() => !EstaOcupado;

        [RelayCommand(CanExecute = nameof(PuedeTocar))]
        private async Task GuardarAsync()
        {
            EstaOcupado = true;
            try
            {
                _noticia.Titulo = Titulo ?? "";
                _noticia.Contenido = Contenido ?? "";
                _noticia.Activa = Activa;

                var resultado = await _admin.GuardarAsync(_noticia, _nuevaImagen);
                await Shell.Current.DisplayAlertAsync(resultado.EsExito ? "Listo" : "Aviso", resultado.Mensaje, "Aceptar");
                if (resultado.EsExito)
                    await Shell.Current.GoToAsync("..");
            }
            finally
            {
                EstaOcupado = false;
            }
        }

        [RelayCommand(CanExecute = nameof(PuedeTocar))]
        private async Task EliminarAsync()
        {
            bool confirmar = await Shell.Current.DisplayAlertAsync("Eliminar noticia",
                "¿Eliminar esta noticia? Si solo quieres que no se vea, desactiva «Publicada».",
                "Eliminar", "Cancelar");
            if (!confirmar) return;

            EstaOcupado = true;
            try
            {
                var resultado = await _admin.EliminarAsync(_noticia);
                await Shell.Current.DisplayAlertAsync(resultado.EsExito ? "Listo" : "Error", resultado.Mensaje, "Aceptar");
                if (resultado.EsExito)
                    await Shell.Current.GoToAsync("..");
            }
            finally
            {
                EstaOcupado = false;
            }
        }
    }
}