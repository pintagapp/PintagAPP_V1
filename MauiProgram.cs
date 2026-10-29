using Microsoft.Extensions.Logging;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.CasosUso.Noticias;
using PintagAPP.CasosUso.Ofertas;
using PintagAPP.CasosUso.Perfiles;
using PintagAPP.Infraestructura.Supabase;
using PintagAPP.Presentacion.ViewModels;
using PintagAPP.Presentacion.Views;

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
            builder.Services.AddSingleton<IPerfilRepositorio, SupabasePerfilRepositorio>();
            builder.Services.AddSingleton<IAlmacenamientoServicio, SupabaseAlmacenamiento>();
            builder.Services.AddSingleton<IOfertaRepositorio, SupabaseOfertaRepositorio>();

            // Estado de la app
            builder.Services.AddSingleton<SesionActual>();

            // Casos de uso
            builder.Services.AddTransient<RegistrarUsuario>();
            builder.Services.AddTransient<IniciarSesion>();
            builder.Services.AddTransient<ListarCategorias>();
            builder.Services.AddTransient<ControlSesion>();
            builder.Services.AddTransient<ListarNoticias>();
            builder.Services.AddTransient<GestionarMiPerfil>();
            builder.Services.AddTransient<GestionarMisOfertas>();

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
            builder.Services.AddTransient<OfrecerViewModel>();
            builder.Services.AddTransient<OfrecerPage>();
            builder.Services.AddTransient<MisOfertasViewModel>();
            builder.Services.AddTransient<MisOfertasPage>();
            builder.Services.AddTransient<OfertaFormViewModel>();
            builder.Services.AddTransient<OfertaFormPage>();

            return builder.Build();
        }
    }
}
