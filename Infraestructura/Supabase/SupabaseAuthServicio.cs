using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Networking;
using PintagAPP.CasosUso;
using PintagAPP.CasosUso.Cuenta;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseAuthServicio : IAuthServicio
    {
        private const string ClaveRefreshToken = "supabase_refresh_token";
        private const string ClaveUsuario = "usuario_guardado";   // nombre y rol, para abrir la app sin internet

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
                var detalle = await respuesta.Content.ReadAsStringAsync();

                if (!respuesta.IsSuccessStatusCode)
                    return Resultado.Error(TraducirErrorRegistro(detalle));

                // Si el correo ya tiene cuenta, Supabase responde "bien" pero con identities vacío
                if (CorreoYaRegistrado(detalle))
                    return Resultado.Error("Este correo ya está registrado. Inicia sesión o usa «¿Olvidaste tu contraseña?».");

                return Resultado.Exito("¡Cuenta creada! Revisa tu correo y confirma tu cuenta para iniciar sesión.");
            }
            catch (HttpRequestException)
            {
                return Resultado.Error("No hay conexión a internet. Inténtalo de nuevo.");
            }
        }

        // La respuesta trae el usuario directo o dentro de "user", según la configuración de Supabase
        private static bool CorreoYaRegistrado(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var raiz = doc.RootElement;
                var usuario = raiz.TryGetProperty("user", out var u) && u.ValueKind == JsonValueKind.Object ? u : raiz;

                return usuario.TryGetProperty("identities", out var identidades)
                       && identidades.ValueKind == JsonValueKind.Array
                       && identidades.GetArrayLength() == 0;
            }
            catch (JsonException)
            {
                return false;
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

        // ---------- OLVIDÉ MI CONTRASEÑA ----------
        // Supabase envía un correo con un enlace a la página web (GitHub Pages), donde se crea la nueva clave.
        // Por seguridad, Supabase responde igual aunque el correo no esté registrado.
        public async Task<Resultado> RecuperarContrasenaAsync(string correo)
        {
            try
            {
                var respuesta = await _http.Cliente.PostAsJsonAsync("auth/v1/recover", new { email = correo });

                if (respuesta.IsSuccessStatusCode)
                    return Resultado.Exito("Si el correo está registrado, te enviamos un enlace para crear una nueva contraseña. Revisa también la carpeta de spam.");

                var detalle = await respuesta.Content.ReadAsStringAsync();
                if ((int)respuesta.StatusCode == 429 || detalle.Contains("rate limit"))
                    return Resultado.Error("Ya se enviaron varios correos. Espera unos minutos e inténtalo de nuevo.");

                return Resultado.Error("No se pudo enviar el correo. Inténtalo más tarde.");
            }
            catch (HttpRequestException)
            {
                return Resultado.Error("No hay conexión a internet. Inténtalo de nuevo.");
            }
        }

        // ---------- RESTAURAR SESIÓN (al abrir la app) ----------
        public async Task<bool> RestaurarSesionAsync()
        {
            string? refreshToken;
            try
            {
                refreshToken = await SecureStorage.Default.GetAsync(ClaveRefreshToken);
            }
            catch (Exception)
            {
                // La sesión guardada no se puede leer (por ejemplo, restaurada de un respaldo
                // de otra instalación): se descarta y la persona inicia sesión de nuevo
                SecureStorage.Default.RemoveAll();
                return false;
            }

            if (string.IsNullOrEmpty(refreshToken))
                return false;

            // Sin internet: se abre con los datos guardados y se renueva cuando vuelva la conexión
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return await RestaurarSinInternetAsync();

            try
            {
                return await RenovarSesionAsync(refreshToken);
            }
            catch (Exception)
            {
                // Hay señal pero Supabase no respondió: igual se abre con los datos guardados
                return await RestaurarSinInternetAsync();
            }
        }

        // Canjea el refresh token por uno nuevo. Devuelve false si Supabase ya no lo acepta.
        private async Task<bool> RenovarSesionAsync(string refreshToken)
        {
            var respuesta = await _http.Cliente.PostAsJsonAsync(
                "auth/v1/token?grant_type=refresh_token",
                new { refresh_token = refreshToken });

            if (!respuesta.IsSuccessStatusCode)
                return false;

            var token = await respuesta.Content.ReadFromJsonAsync<TokenDto>();
            if (token is null)
                return false;

            var resultado = await CompletarSesionAsync(token);
            return resultado.EsExito;
        }

        // ---------- MODO OFFLINE ----------
        // Muestra la sesión con el nombre y rol guardados (sin token, solo se leen las copias guardadas)
        private async Task<bool> RestaurarSinInternetAsync()
        {
            try
            {
                var json = await SecureStorage.Default.GetAsync(ClaveUsuario);
                if (string.IsNullOrEmpty(json)) return false;

                var dto = JsonSerializer.Deserialize<UsuarioDto>(json);
                if (dto is null) return false;

                _sesion.Iniciar(new Usuario { Id = dto.Id, NombresCompletos = dto.NombresCompletos, Rol = dto.Rol });

                // Cuando vuelva el internet, se renueva la sesión de verdad
                Connectivity.ConnectivityChanged -= AlCambiarConexion;
                Connectivity.ConnectivityChanged += AlCambiarConexion;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void AlCambiarConexion(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess != NetworkAccess.Internet) return;
            Connectivity.ConnectivityChanged -= AlCambiarConexion;
            _ = RenovarAlVolverInternetAsync();
        }

        private async Task RenovarAlVolverInternetAsync()
        {
            try
            {
                var refreshToken = await SecureStorage.Default.GetAsync(ClaveRefreshToken);
                if (string.IsNullOrEmpty(refreshToken) || !await RenovarSesionAsync(refreshToken))
                    LimpiarSesionLocal();   // la sesión ya no es válida (por ejemplo, cambió la contraseña)
            }
            catch (Exception)
            {
                // La señal todavía es inestable: se vuelve a intentar en el próximo cambio de conexión
                Connectivity.ConnectivityChanged -= AlCambiarConexion;
                Connectivity.ConnectivityChanged += AlCambiarConexion;
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

            LimpiarSesionLocal();
        }

        // ---------- ELIMINAR CUENTA (Art. 15 LOPDP) ----------
        // La función de la BD borra la cuenta y, en cascada, perfil, ofertas, contactos, calificaciones y denuncias
        public async Task<Resultado> EliminarCuentaAsync()
        {
            try
            {
                var respuesta = await _http.Cliente.PostAsJsonAsync("rest/v1/rpc/eliminar_mi_cuenta", new { });
                if (!respuesta.IsSuccessStatusCode)
                    return Resultado.Error("No se pudo eliminar la cuenta. Inténtalo de nuevo.");
            }
            catch (HttpRequestException)
            {
                return Resultado.Error("No hay conexión a internet. Inténtalo de nuevo.");
            }

            // La cuenta ya no existe: se limpia la sesión del celular
            LimpiarSesionLocal();
            return Resultado.Exito("Tu cuenta y todos tus datos fueron eliminados.");
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

            // Para poder abrir la app con la sesión iniciada aunque no haya internet
            await SecureStorage.Default.SetAsync(ClaveUsuario, JsonSerializer.Serialize(dto));

            return Resultado.Exito($"Bienvenido, {dto.NombresCompletos}");
        }

        // Borra todo lo de la sesión que está en el celular (no toca las ofertas guardadas, que son públicas)
        private void LimpiarSesionLocal()
        {
            Connectivity.ConnectivityChanged -= AlCambiarConexion;
            SecureStorage.Default.Remove(ClaveRefreshToken);
            SecureStorage.Default.Remove(ClaveUsuario);
            _http.EstablecerToken(null);
            _http.BorrarPrivado();
            _sesion.Cerrar();
        }

        private static string TraducirErrorRegistro(string detalle)
        {
            if (detalle.Contains("Database error"))
                return "Esta cédula ya tiene una cuenta. Si es tuya, inicia sesión o usa «¿Olvidaste tu contraseña?».";
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