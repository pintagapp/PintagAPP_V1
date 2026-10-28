using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Noticias;

namespace PintagAPP.Presentacion.ViewModels
{
    // Lo que muestra cada tarjeta de noticia en pantalla
    public record NoticiaItem(string Titulo, string Contenido, string? ImagenUrl, string Fecha)
    {
        public bool TieneImagen => !string.IsNullOrEmpty(ImagenUrl);
    }

    public partial class NoticiasViewModel : ObservableObject
    {
        private readonly ListarNoticias _listarNoticias;

        public NoticiasViewModel(ListarNoticias listarNoticias)
        {
            _listarNoticias = listarNoticias;
        }

        public ObservableCollection<NoticiaItem> Noticias { get; } = new();

        [ObservableProperty] private bool estaCargando;
        [ObservableProperty] private string mensaje = "";
        [ObservableProperty] private bool sinNoticias;

        // Primera vez que se abre la pestaña
        public async Task CargarAsync()
        {
            if (Noticias.Count > 0) return;
            await RefrescarAsync();
        }

        // Al deslizar hacia abajo para actualizar
        [RelayCommand]
        private async Task RefrescarAsync()
        {
            EstaCargando = true;
            Mensaje = "";
            try
            {
                var noticias = await _listarNoticias.EjecutarAsync(20);
                Noticias.Clear();
                foreach (var n in noticias)
                    Noticias.Add(new NoticiaItem(n.Titulo, n.Contenido, n.ImagenUrl,
                                                 n.FechaPublicacion.ToLocalTime().ToString("dd/MM/yyyy")));
                SinNoticias = Noticias.Count == 0;
            }
            catch (Exception)
            {
                Mensaje = "No se pudieron cargar las noticias. Revisa tu conexión.";
            }
            finally
            {
                EstaCargando = false;
            }
        }
    }
}