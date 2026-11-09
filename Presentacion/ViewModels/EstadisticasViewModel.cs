using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Perfiles;

namespace PintagAPP.Presentacion.ViewModels
{
    public record IndicadorItem(string Emoji, string Titulo, string Valor);
    public record CategoriaEstadisticaItem(string Nombre, string Detalle);

    public partial class EstadisticasViewModel : ObservableObject
    {
        private readonly VerEstadisticas _ver;

        public EstadisticasViewModel(VerEstadisticas ver)
        {
            _ver = ver;
        }

        public ObservableCollection<IndicadorItem> Indicadores { get; } = new();
        public ObservableCollection<CategoriaEstadisticaItem> Categorias { get; } = new();

        [ObservableProperty] private string aviso = "";
        [ObservableProperty] private bool estaCargando;

        public async Task CargarAsync()
        {
            EstaCargando = true;
            Aviso = "";
            Indicadores.Clear();
            Categorias.Clear();
            try
            {
                var e = await _ver.ObtenerGeneralesAsync();
                if (e is null)
                {
                    Aviso = "Esta sección es solo para el administrador.";
                    return;
                }

                Indicadores.Add(new("👥", "Usuarios registrados", e.TotalUsuarios.ToString()));
                Indicadores.Add(new("🏪", "Oferentes aprobados", e.OferentesAprobados.ToString()));
                Indicadores.Add(new("⏳", "Perfiles pendientes", e.OferentesPendientes.ToString()));
                Indicadores.Add(new("⛔", "Perfiles bloqueados", e.OferentesBloqueados.ToString()));
                Indicadores.Add(new("📦", "Ofertas activas",
                    $"{e.OfertasActivas} ({e.OfertasProductos} prod. / {e.OfertasServicios} serv.)"));
                Indicadores.Add(new("📞", "Contactos (WhatsApp y llamadas)", e.TotalContactos.ToString()));
                Indicadores.Add(new("⭐", "Calificaciones", e.TotalCalificaciones.ToString()));
                Indicadores.Add(new("📈", "Promedio general", e.PromedioTexto));
                Indicadores.Add(new("🚩", "Denuncias pendientes", e.DenunciasPendientes.ToString()));

                var categorias = await _ver.ObtenerPorCategoriaAsync();
                foreach (var c in categorias)
                    Categorias.Add(new CategoriaEstadisticaItem(c.Categoria,
                        $"{c.OfertasActivas} ofertas · {c.Oferentes} oferentes"));

                Aviso = "Actualizado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            }
            catch (Exception)
            {
                Aviso = "No se pudieron cargar las estadísticas. Revisa tu conexión.";
            }
            finally
            {
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private Task RecargarAsync() => CargarAsync();
    }
}