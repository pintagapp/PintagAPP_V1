using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Noticias;
using PintagAPP.CasosUso.Ofertas;
using PintagAPP.Infraestructura.Supabase;

namespace PintagAPP.Presentacion.ViewModels
{
    // Lo que muestra cada cuadrito de categoría en pantalla
    public record CategoriaItem(long Id, string Nombre, string Emoji);

    public partial class InicioViewModel : ObservableObject
    {
        private readonly ListarCategorias _listarCategorias;
        private readonly ListarNoticias _listarNoticias;
        private readonly SesionActual _sesion;

        public InicioViewModel(ListarCategorias listarCategorias, ListarNoticias listarNoticias, SesionActual sesion)
        {
            _listarCategorias = listarCategorias;
            _listarNoticias = listarNoticias;
            _sesion = sesion;
        }

        public ObservableCollection<CategoriaItem> Categorias { get; } = new();
        public ObservableCollection<NoticiaItem> Noticias { get; } = new();
        public string PortadaUrl => SupabaseConfig.PortadaUrl;

        [ObservableProperty] private string saludo = "¡Hola, Píntag!";
        [ObservableProperty] private bool estaCargando;
        [ObservableProperty] private string mensaje = "";
        [ObservableProperty] private bool sinNoticias;
        [ObservableProperty] private CategoriaItem? categoriaSeleccionada;

        public async Task CargarAsync()
        {
            Saludo = _sesion.EstaIniciada
                ? $"¡Hola, {_sesion.Usuario!.NombresCompletos.Split(' ')[0]}!"
                : "¡Hola, Píntag!";

            await CargarNoticiasAsync();   // cada vez, para que aparezcan las nuevas

            if (Categorias.Count > 0 || EstaCargando) return;   // categorías ya cargadas

            EstaCargando = true;
            Mensaje = "";
            try
            {
                var lista = await _listarCategorias.EjecutarAsync();
                foreach (var c in lista)
                    Categorias.Add(new CategoriaItem(c.Id, c.Nombre, EmojiDe(c.Icono)));
            }
            catch (Exception)
            {
                Mensaje = "No se pudieron cargar las categorías. Revisa tu conexión.";
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task CargarNoticiasAsync()
        {
            try
            {
                var lista = await _listarNoticias.EjecutarAsync(3);
                Noticias.Clear();
                foreach (var n in lista)
                    Noticias.Add(new NoticiaItem(n.Titulo, n.Contenido, n.ImagenUrl,
                                                 n.FechaPublicacion.ToLocalTime().ToString("dd/MM/yyyy")));
            }
            catch (Exception)
            {
                // Si fallan las noticias, el resto del inicio sigue funcionando
            }
            SinNoticias = Noticias.Count == 0;
        }

        // Al tocar una categoría se abren sus ofertas
        async partial void OnCategoriaSeleccionadaChanged(CategoriaItem? value)
        {
            if (value is null) return;
            CategoriaSeleccionada = null;
            await Shell.Current.GoToAsync("OfertasCategoriaPage",
                new Dictionary<string, object> { ["id"] = value.Id, ["nombre"] = value.Nombre });
        }

        // La barra de búsqueda del inicio lleva a la pestaña Buscar
        [RelayCommand]
        private async Task IrABuscarAsync()
        {
            await Shell.Current.GoToAsync("//BuscarPage");
        }

        // El campo "icono" de la BD se traduce aquí a un dibujo (es un tema de presentación)
        private static string EmojiDe(string icono) => icono switch
        {
            "turismo" => "🏔️",
            "chagra" => "🐎",
            "ganaderia" => "🐄",
            "agro" => "🌾",
            "alimentos" => "🍞",
            "artesanias" => "🏺",
            "eventos" => "🎉",
            "mecanica" => "🔧",
            "oficios" => "🔨",
            "electricidad" => "⚡",
            "tecnologia" => "💻",
            "hogar" => "🏠",
            "belleza" => "✂️",
            "transporte" => "🚚",
            "clases" => "📚",
            _ => "📦"
        };
    }
}