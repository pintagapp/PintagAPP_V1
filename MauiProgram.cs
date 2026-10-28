using Microsoft.Extensions.Logging;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
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

            // Casos de uso
            builder.Services.AddTransient<RegistrarUsuario>();

            // Presentación
            builder.Services.AddTransient<RegistroViewModel>();
            builder.Services.AddTransient<RegistroPage>();

            return builder.Build();
        }
    }
}
