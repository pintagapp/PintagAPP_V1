using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Noticias;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Presentacion.ViewModels
{
    public record AdminNoticiaItem(Noticia Noticia)
    {
        public string Titulo => Noticia.Titulo;
        public string Estado => Noticia.Activa ? "🟢 Publicada" : "⏸️ Oculta";
        public string Fecha => Noticia.FechaPublicacion.ToLocalTime().ToString("dd/MM/yyyy");
        public string? ImagenUrl => Noticia.ImagenUrl;
        public bool TieneImagen => !string.IsNullOrEmpty(ImagenUrl);
    }

    public partial class AdminNoticiasViewModel : ObservableObject
    {
        private readonly AdministrarNoticias _admin;

        public AdminNoticiasViewModel(AdministrarNoticias admin)
        {
            _admin = admin;
        }

        public ObservableCollection<AdminNoticiaItem> Noticias { get; } = new();

        [ObservableProperty] private AdminNoticiaItem? noticiaSeleccionada;
        [ObservableProperty] private string aviso = "";
        [ObservableProperty] private bool estaCargando;

        public async Task CargarAsync()
        {
            EstaCargando = true;
            Noticias.Clear();
            try
            {
                var lista = await _admin.ObtenerTodasAsync();
                foreach (var n in lista)
                    Noticias.Add(new AdminNoticiaItem(n));
                Aviso = Noticias.Count == 0
                    ? "Aún no hay noticias. Toca «Nueva noticia» para publicar la primera."
                    : "Toca una noticia para editarla, ocultarla o eliminarla.";
            }
            catch (Exception)
            {
                Aviso = "No se pudieron cargar las noticias. Revisa tu conexión.";
            }
            finally
            {
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private async Task NuevaAsync()
        {
            await Shell.Current.GoToAsync("NoticiaFormPage");
        }

        async partial void OnNoticiaSeleccionadaChanged(AdminNoticiaItem? value)
        {
            if (value is null) return;
            NoticiaSeleccionada = null;
            await Shell.Current.GoToAsync("NoticiaFormPage",
                new Dictionary<string, object> { ["noticia"] = value.Noticia });
        }
    }
}