using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PintagAPP.CasosUso.Calificaciones;

namespace PintagAPP.Presentacion.ViewModels
{
    public record PerfilPorCalificar(long Id, string Nombre, string Barrio, string? FotoUrl)
    {
        public bool TieneFoto => !string.IsNullOrEmpty(FotoUrl);
    }

    public partial class PorCalificarViewModel : ObservableObject
    {
        private readonly CalificarOferentes _calificar;

        public PorCalificarViewModel(CalificarOferentes calificar)
        {
            _calificar = calificar;
        }

        public ObservableCollection<PerfilPorCalificar> Perfiles { get; } = new();

        [ObservableProperty] private PerfilPorCalificar? perfilSeleccionado;
        [ObservableProperty] private string aviso = "";
        [ObservableProperty] private bool estaCargando;

        // Se recarga cada vez que aparece (al volver de calificar, ese oferente ya no sale)
        public async Task CargarAsync()
        {
            EstaCargando = true;
            Perfiles.Clear();
            try
            {
                var lista = await _calificar.ObtenerPorCalificarAsync();
                foreach (var p in lista)
                    Perfiles.Add(new PerfilPorCalificar(p.Id, p.NombreVisible, p.BarrioNombre ?? "", p.FotoUrl));

                Aviso = Perfiles.Count == 0
                    ? "✅ No tienes calificaciones pendientes."
                    : "Estos oferentes te atendieron. Tu opinión ayuda a otros vecinos de Píntag.";
            }
            catch (Exception)
            {
                Aviso = "No se pudo cargar la lista. Revisa tu conexión.";
            }
            finally
            {
                EstaCargando = false;
            }
        }

        async partial void OnPerfilSeleccionadoChanged(PerfilPorCalificar? value)
        {
            if (value is null) return;
            PerfilSeleccionado = null;
            await Shell.Current.GoToAsync("CalificarPage",
                new Dictionary<string, object> { ["perfilId"] = value.Id, ["nombre"] = value.Nombre });
        }
    }
}