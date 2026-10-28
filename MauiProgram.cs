using Microsoft.Extensions.Logging;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.CasosUso.Ofertas;
using PintagAPP.Infraestructura.Supabase;
using PintagAPP.Presentacion.ViewModels;
using PintagAPP.Presentacion.Views;
using PintagAPP.CasosUso.Noticias;

namespace PintagAPP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            // Infraestructura (Supabase)
            builder.Services.AddSingleton<SupabaseHttp>();
            builder.Services.AddSingleton<IAuthServicio, SupabaseAuthServicio>();
            builder.Services.AddSingleton<ICatalogoRepositorio, SupabaseCatalogoRepositorio>();
            builder.Services.AddSingleton<INoticiaRepositorio, SupabaseNoticiaRepositorio>();

            // Estado de la app
            builder.Services.AddSingleton<SesionActual>();

            // Casos de uso
            builder.Services.AddTransient<RegistrarUsuario>();
            builder.Services.AddTransient<IniciarSesion>();
            builder.Services.AddTransient<ListarCategorias>();
            builder.Services.AddTransient<ControlSesion>();
            builder.Services.AddTransient<ListarNoticias>();
            

            // Presentación
            builder.Services.AddTransient<RegistroViewModel>();
            builder.Services.AddTransient<RegistroPage>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<InicioViewModel>();
            builder.Services.AddTransient<InicioPage>();
            builder.Services.AddTransient<BuscarPage>();
            builder.Services.AddTransient<NoticiasViewModel>();
            builder.Services.AddTransient<NoticiasPage>();
            builder.Services.AddTransient<MiPerfilViewModel>();
            builder.Services.AddTransient<MiPerfilPage>();

            return builder.Build();
        }
    }
}
