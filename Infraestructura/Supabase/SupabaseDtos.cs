using System.Text.Json.Serialization;

namespace PintagAPP.Infraestructura.Supabase
{
    // Respuesta de /auth/v1/token
    public class TokenDto
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
        [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = "";
        [JsonPropertyName("user")] public UsuarioAuthDto Usuario { get; set; } = new();
    }

    // El "user" que viene dentro del token
    public class UsuarioAuthDto
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
    }

    // Fila de la tabla usuarios
    public class UsuarioDto
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("nombres_completos")] public string NombresCompletos { get; set; } = "";
        [JsonPropertyName("rol")] public string Rol { get; set; } = "";
    }
}