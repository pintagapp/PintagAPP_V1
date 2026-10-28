using System.Net.Http.Json;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseCatalogoRepositorio : ICatalogoRepositorio
    {
        private readonly SupabaseHttp _http;

        public SupabaseCatalogoRepositorio(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task<List<Categoria>> ObtenerCategoriasAsync()
        {
            // GET /rest/v1/categorias  → solo activas, ordenadas
            var filas = await _http.Cliente.GetFromJsonAsync<List<CategoriaDto>>(
                "rest/v1/categorias?select=id,nombre,icono,orden&activa=eq.true&order=orden.asc");

            return (filas ?? new List<CategoriaDto>())
                .Select(f => new Categoria
                {
                    Id = f.Id,
                    Nombre = f.Nombre,
                    Icono = f.Icono,
                    Orden = f.Orden
                })
                .ToList();
        }
    }
}
