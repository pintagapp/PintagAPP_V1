using System.Net.Http.Json;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseOfertaRepositorio : IOfertaRepositorio
    {
        private const string Columnas =
            "id,perfil_id,posicion,categoria_id,tipo,titulo,descripcion,unidades_por_precio,precio,a_convenir,activa," +
            "oferta_fotos(id,url,orden)";

        // Datos públicos del oferente (sin teléfono: los visitantes sin cuenta no pueden verlo)
        private const string ColumnasPublicas = Columnas +
            ",perfiles_oferente!inner(id,nombre_persona,nombre_negocio,descripcion,foto_url,estado,barrios(nombre))";

        // Se filtra explícitamente porque el admin, por RLS, también vería las pendientes y pausadas
        private const string FiltroPublicas = "activa=eq.true&perfiles_oferente.estado=eq.aprobado";

        private readonly SupabaseHttp _http;

        public SupabaseOfertaRepositorio(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task<List<Oferta>> ObtenerDePerfilAsync(long perfilId)
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<OfertaDto>>(
                $"rest/v1/ofertas?perfil_id=eq.{perfilId}&select={Columnas}&order=posicion.asc");

            return (filas ?? new List<OfertaDto>()).Select(Mapear).ToList();
        }

        public async Task<Oferta> GuardarAsync(Oferta oferta)
        {
            var datos = new Dictionary<string, object?>
            {
                ["categoria_id"] = oferta.CategoriaId,
                ["tipo"] = oferta.Tipo,
                ["titulo"] = oferta.Titulo,
                ["descripcion"] = oferta.Descripcion,
                ["unidades_por_precio"] = oferta.UnidadesPorPrecio,
                ["precio"] = oferta.Precio,
                ["a_convenir"] = oferta.AConvenir,
                ["activa"] = oferta.Activa
            };

            HttpRequestMessage solicitud;
            if (oferta.EsNueva)
            {
                datos["perfil_id"] = oferta.PerfilId;
                datos["posicion"] = oferta.Posicion;
                solicitud = new HttpRequestMessage(HttpMethod.Post, "rest/v1/ofertas");
            }
            else
            {
                solicitud = new HttpRequestMessage(HttpMethod.Patch, $"rest/v1/ofertas?id=eq.{oferta.Id}");
            }

            solicitud.Content = JsonContent.Create(datos);
            solicitud.Headers.Add("Prefer", "return=representation");

            var respuesta = await _http.Cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            var filas = await respuesta.Content.ReadFromJsonAsync<List<OfertaDto>>();
            var guardada = Mapear(filas!.First());
            oferta.Id = guardada.Id;
            oferta.Posicion = guardada.Posicion;
            return guardada;
        }

        public async Task EliminarAsync(long ofertaId)
        {
            var respuesta = await _http.Cliente.DeleteAsync($"rest/v1/ofertas?id=eq.{ofertaId}");
            respuesta.EnsureSuccessStatusCode();
        }

        public async Task AgregarFotoAsync(long ofertaId, string url, int orden)
        {
            var respuesta = await _http.Cliente.PostAsJsonAsync("rest/v1/oferta_fotos",
                new { oferta_id = ofertaId, url, orden });
            respuesta.EnsureSuccessStatusCode();
        }

        public async Task EliminarFotoAsync(long fotoId)
        {
            var respuesta = await _http.Cliente.DeleteAsync($"rest/v1/oferta_fotos?id=eq.{fotoId}");
            respuesta.EnsureSuccessStatusCode();
        }

        // ---------- PÚBLICO ----------
        public async Task<List<Oferta>> ObtenerPublicasPorCategoriaAsync(long categoriaId)
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<OfertaDto>>(
                $"rest/v1/ofertas?categoria_id=eq.{categoriaId}&{FiltroPublicas}&select={ColumnasPublicas}&order=id.desc");

            return (filas ?? new List<OfertaDto>()).Select(Mapear).ToList();
        }

        public async Task<Oferta?> ObtenerPublicaAsync(long ofertaId)
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<OfertaDto>>(
                $"rest/v1/ofertas?id=eq.{ofertaId}&{FiltroPublicas}&select={ColumnasPublicas}");

            var fila = filas?.FirstOrDefault();
            return fila is null ? null : Mapear(fila);
        }

        private static Oferta Mapear(OfertaDto f) => new()
        {
            Id = f.Id,
            PerfilId = f.PerfilId,
            Posicion = f.Posicion,
            CategoriaId = f.CategoriaId,
            Tipo = f.Tipo,
            Titulo = f.Titulo,
            Descripcion = f.Descripcion,
            UnidadesPorPrecio = f.UnidadesPorPrecio,
            Precio = f.Precio,
            AConvenir = f.AConvenir,
            Activa = f.Activa,
            Oferente = f.Perfil is null ? null : new PerfilOferente
            {
                Id = f.Perfil.Id,
                NombrePersona = f.Perfil.NombrePersona,
                NombreNegocio = f.Perfil.NombreNegocio,
                Descripcion = f.Perfil.Descripcion,
                FotoUrl = f.Perfil.FotoUrl,
                Estado = f.Perfil.Estado,
                BarrioNombre = f.Perfil.Barrio?.Nombre
            },
            Fotos = (f.Fotos ?? new List<FotoOfertaDto>())
                    .OrderBy(x => x.Orden)
                    .Select(x => new FotoOferta { Id = x.Id, Url = x.Url, Orden = x.Orden })
                    .ToList()
        };
    }
}