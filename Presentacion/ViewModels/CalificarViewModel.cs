using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Calificaciones;
using static Android.Icu.Text.CaseMap;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class CalificarViewModel : ObservableObject, IQueryAttributable
    {
        private readonly CalificarOferentes _calificar;
        private long _perfilId;

        public CalificarViewModel(CalificarOferentes calificar)
        {
            _calificar = calificar;
        }

        [ObservableProperty] private string nombreOferente = "";
        [ObservableProperty] private string titulo = "Calificar";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Estrella1))]
        [NotifyPropertyChangedFor(nameof(Estrella2))]
        [NotifyPropertyChangedFor(nameof(Estrella3))]
        [NotifyPropertyChangedFor(nameof(Estrella4))]
        [NotifyPropertyChangedFor(nameof(Estrella5))]
        [NotifyPropertyChangedFor(nameof(TextoEstrellas))]
        private int estrellas;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ContadorTexto))]
        private string comentario = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
        private bool estaOcupado;

        // Cada estrella se pinta llena o vacía según lo elegido
        public string Estrella1 => Estrellas >= 1 ? "★" : "☆";
        public string Estrella2 => Estrellas >= 2 ? "★" : "☆";
        public string Estrella3 => Estrellas >= 3 ? "★" : "☆";
        public string Estrella4 => Estrellas >= 4 ? "★" : "☆";
        public string Estrella5 => Estrellas >= 5 ? "★" : "☆";

        public string TextoEstrellas => Estrellas switch
        {
            1 => "Malo",
            2 => "Regular",
            3 => "Bueno",
            4 => "Muy bueno",
            5 => "Excelente",
            _ => "Toca las estrellas para calificar"
        };

        public string ContadorTexto => $"{Comentario?.Length ?? 0}/{CalificarOferentes.MaximoComentario}";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("perfilId", out var id))
                _perfilId = Convert.ToInt64(id);
            if (query.TryGetValue("nombre", out var nombre))
                NombreOferente = nombre?.ToString() ?? "";
        }

        // Si ya había calificado, se cargan sus estrellas y comentario para editarlos
        public async Task CargarAsync()
        {
            try
            {
                var mia = await _calificar.ObtenerMiCalificacionAsync(_perfilId);
                if (mia is not null)
                {
                    Titulo = "Editar mi calificación";
                    Estrellas = mia.Estrellas;
                    Comentario = mia.Comentario ?? "";
                }
            }
            catch (Exception)
            {
                // Si falla, simplemente empieza en blanco
            }
        }

        [RelayCommand]
        private void ElegirEstrellas(string valor)
        {
            Estrellas = int.Parse(valor);
        }

        private bool PuedeGuardar() => !EstaOcupado;

        [RelayCommand(CanExecute = nameof(PuedeGuardar))]
        private async Task GuardarAsync()
        {
            EstaOcupado = true;
            try
            {
                var resultado = await _calificar.GuardarAsync(_perfilId, Estrellas, Comentario);
                await Shell.Current.DisplayAlertAsync(resultado.EsExito ? "Listo" : "Aviso", resultado.Mensaje, "Aceptar");
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
