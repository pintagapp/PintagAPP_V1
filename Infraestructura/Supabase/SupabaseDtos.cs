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
    public class BarrioDto
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("nombre")] public string Nombre { get; set; } = "";
    }
    // Fila de la tabla perfil_oferente
    public class PerfilDto
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("usuario_id")] public Guid UsuarioId { get; set; }
        [JsonPropertyName("nombre_persona")] public string NombrePersona { get; set; } = "";
        [JsonPropertyName("nombre_negocio")] public string? NombreNegocio { get; set; }
        [JsonPropertyName("telefono")] public string Telefono { get; set; } = "";
        [JsonPropertyName("barrio_id")] public long BarrioId { get; set; }
        [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = "";
        [JsonPropertyName("foto_url")] public string? FotoUrl { get; set; }
        [JsonPropertyName("estado")] public string Estado { get; set; } = "";
        [JsonPropertyName("motivo_estado")] public string? MotivoEstado { get; set; }
    }
    // Fila de la tabla categorias
    public class CategoriaDto
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("nombre")] public string Nombre { get; set; } = "";
        [JsonPropertyName("icono")] public string Icono { get; set; } = "";
        [JsonPropertyName("orden")] public int Orden { get; set; }
    }
    // Fila de la tabla noticias
    public class NoticiaDto
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("titulo")] public string Titulo { get; set; } = "";
        [JsonPropertyName("contenido")] public string Contenido { get; set; } = "";
        [JsonPropertyName("imagen_url")] public string? ImagenUrl { get; set; }
        [JsonPropertyName("fecha_publicacion")] public DateTimeOffset FechaPublicacion { get; set; }
    }
}