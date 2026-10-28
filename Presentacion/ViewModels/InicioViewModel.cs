using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Ofertas;
using PintagAPP.Infraestructura.Supabase;

namespace PintagAPP.Presentacion.ViewModels
{
    // Lo que muestra cada cuadrito de categoría en pantalla
    public record CategoriaItem(long Id, string Nombre, string Emoji);

    public partial class InicioViewModel : ObservableObject
    {
        private readonly ListarCategorias _listarCategorias;
        private readonly SesionActual _sesion;

        public InicioViewModel(ListarCategorias listarCategorias, SesionActual sesion)
        {
            _listarCategorias = listarCategorias;
            _sesion = sesion;
        }

        public ObservableCollection<CategoriaItem> Categorias { get; } = new();
        public string PortadaUrl => SupabaseConfig.PortadaUrl;

        [ObservableProperty] private string saludo = "¡Hola, Píntag!";
        [ObservableProperty] private bool estaCargando;
        [ObservableProperty] private string mensaje = "";

        public async Task CargarAsync()
        {
            Saludo = _sesion.EstaIniciada
                ? $"¡Hola, {_sesion.Usuario!.NombresCompletos.Split(' ')[0]}!"
                : "¡Hola, Píntag!";

            if (Categorias.Count > 0 || EstaCargando) return;   // ya cargadas

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