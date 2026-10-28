using System.Net.Http.Json;
using PintagAPP.CasosUso;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseAuthServicio : IAuthServicio
    {
        private const string ClaveRefreshToken = "supabase_refresh_token";

        private readonly SupabaseHttp _http;
        private readonly SesionActual _sesion;

        public SupabaseAuthServicio(SupabaseHttp http, SesionActual sesion)
        {
            _http = http;
            _sesion = sesion;
        }

        // ---------- REGISTRO ----------
        public async Task<Resultado> RegistrarAsync(string correo, string contrasena, string cedula, string nombresCompletos)
        {
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
                return Resultado.Error(TraducirErrorRegistro(detalle));
            }
            catch (HttpRequestException)
            {
                return Resultado.Error("No hay conexión a internet. Inténtalo de nuevo.");
            }
        }

        // ---------- INICIAR SESIÓN ----------
        public async Task<Resultado> IniciarSesionAsync(string correo, string contrasena)
        {
            try
            {
                var respuesta = await _http.Cliente.PostAsJsonAsync(
                    "auth/v1/token?grant_type=password",
                    new { email = correo, password = contrasena });

                if (!respuesta.IsSuccessStatusCode)
                {
                    var detalle = await respuesta.Content.ReadAsStringAsync();
                    if (detalle.Contains("email_not_confirmed") || detalle.Contains("not confirmed"))
                        return Resultado.Error("Primero confirma tu cuenta con el enlace que te enviamos al correo.");
                    return Resultado.Error("Correo o contraseña incorrectos.");
                }

                var token = await respuesta.Content.ReadFromJsonAsync<TokenDto>();
                if (token is null)
                    return Resultado.Error("No se pudo iniciar sesión. Inténtalo de nuevo.");

                return await CompletarSesionAsync(token);
            }
            catch (HttpRequestException)
            {
                return Resultado.Error("No hay conexión a internet. Inténtalo de nuevo.");
            }
        }

        // ---------- RESTAURAR SESIÓN (al abrir la app) ----------
        public async Task<bool> RestaurarSesionAsync()
        {
            var refreshToken = await SecureStorage.Default.GetAsync(ClaveRefreshToken);
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            try
            {
                var respuesta = await _http.Cliente.PostAsJsonAsync(
                    "auth/v1/token?grant_type=refresh_token",
                    new { refresh_token = refreshToken });

                if (!respuesta.IsSuccessStatusCode)
                {
                    SecureStorage.Default.Remove(ClaveRefreshToken);
                    return false;
                }

                var token = await respuesta.Content.ReadFromJsonAsync<TokenDto>();
                if (token is null)
                    return false;

                var resultado = await CompletarSesionAsync(token);
                return resultado.EsExito;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        // ---------- CERRAR SESIÓN ----------
        public async Task CerrarSesionAsync()
        {
            try
            {
                await _http.Cliente.PostAsync("auth/v1/logout", null);
            }
            catch (HttpRequestException)
            {
                // Si no hay internet, igual se cierra la sesión en el celular
            }

            SecureStorage.Default.Remove(ClaveRefreshToken);
            _http.EstablecerToken(null);
            _sesion.Cerrar();
        }

        // ---------- COMÚN ----------
        // Guarda el token, lee los datos del usuario de la tabla usuarios y abre la sesión
        private async Task<Resultado> CompletarSesionAsync(TokenDto token)
        {
            _http.EstablecerToken(token.AccessToken);
            await SecureStorage.Default.SetAsync(ClaveRefreshToken, token.RefreshToken);

            var filas = await _http.Cliente.GetFromJsonAsync<List<UsuarioDto>>(
                $"rest/v1/usuarios?id=eq.{token.Usuario.Id}&select=id,nombres_completos,rol");

            var dto = filas?.FirstOrDefault();
            if (dto is null)
            {
                _http.EstablecerToken(null);
                return Resultado.Error("No se encontraron los datos de tu cuenta.");
            }

            _sesion.Iniciar(new Usuario
            {
                Id = dto.Id,
                NombresCompletos = dto.NombresCompletos,
                Rol = dto.Rol
            });

            return Resultado.Exito($"Bienvenido, {dto.NombresCompletos}");
        }

        private static string TraducirErrorRegistro(string detalle)
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