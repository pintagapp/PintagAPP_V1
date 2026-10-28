using PintagAPP.CasosUso;
using PintagAPP.CasosUso.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseAuthServicio : IAuthServicio
    {
        private readonly SupabaseHttp _http;

        public SupabaseAuthServicio(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task<Resultado> RegistrarAsync(string correo, string contrasena, string cedula, string nombresCompletos)
        {
            // "data" llega a raw_user_meta_data; el trigger de la BD crea la fila en usuarios
            var cuerpo = new
            {
                email = correo,
                password = contrasena,
                data = new
                {
                    cedula,
                    nombres_completos = nombresCompletos,
                    acepta_aviso = true,
                    aviso_version = SupabaseConfig.AvisoVersion
                }
            };

            try
            {
                var respuesta = await _http.Cliente.PostAsJsonAsync("auth/v1/signup", cuerpo);

                if (respuesta.IsSuccessStatusCode)
                    return Resultado.Exito("¡Cuenta creada! Revisa tu correo y confirma tu cuenta para iniciar sesión.");

                var detalle = await respuesta.Content.ReadAsStringAsync();
                return Resultado.Error(TraducirError(detalle));
            }
            catch (HttpRequestException)
            {
                return Resultado.Error("No hay conexión a internet. Inténtalo de nuevo.");
            }
        }

        private static string TraducirError(string detalle)
        {
            if (detalle.Contains("Database error"))
                return "No se pudo crear la cuenta. Es posible que esta cédula ya esté registrada.";
            if (detalle.Contains("rate limit"))
                return "Demasiados intentos. Espera unos minutos e inténtalo de nuevo.";
            if (detalle.Contains("Password"))
                return "La contraseña no cumple los requisitos de seguridad.";
            if (detalle.Contains("email"))
                return "El correo electrónico no es válido.";
            return "No se pudo crear la cuenta. Inténtalo más tarde.";
        }
    }
}