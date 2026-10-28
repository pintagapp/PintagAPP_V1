using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseHttp
    {
        public HttpClient Cliente { get; }

        public SupabaseHttp()
        {
            Cliente = new HttpClient { BaseAddress = new Uri(SupabaseConfig.Url + "/") };
            // La clave publishable va SIEMPRE en "apikey" (nunca en Authorization)
            Cliente.DefaultRequestHeaders.Add("apikey", SupabaseConfig.PublishableKey);
        }

        // Después del login: el token del usuario va en Authorization
        public void EstablecerToken(string? token)
        {
            Cliente.DefaultRequestHeaders.Authorization =
                token is null ? null : new AuthenticationHeaderValue("Bearer", token);
        }
    }
}