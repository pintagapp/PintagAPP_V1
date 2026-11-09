using System.Net.Http.Json;
using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Infraestructura.Supabase
{
    public class SupabaseEstadisticaRepositorio : IEstadisticaRepositorio
    {
        private readonly SupabaseHttp _http;

        public SupabaseEstadisticaRepositorio(SupabaseHttp http)
        {
            _http = http;
        }

        public async Task<Estadisticas?> ObtenerGeneralesAsync()
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<EstadisticasDto>>(
                "rest/v1/vista_estadisticas?select=*");

            var f = filas?.FirstOrDefault();
            if (f is null) return null;   // no es admin

            return new Estadisticas
            {
                TotalUsuarios = f.TotalUsuarios,
                OferentesAprobados = f.OferentesAprobados,
                OferentesPendientes = f.OferentesPendientes,
                OferentesBloqueados = f.OferentesBloqueados,
                OfertasActivas = f.OfertasActivas,
                OfertasProductos = f.OfertasProductos,
                OfertasServicios = f.OfertasServicios,
                TotalContactos = f.TotalContactos,
                TotalCalificaciones = f.TotalCalificaciones,
                PromedioGeneral = f.PromedioGeneral,
                DenunciasPendientes = f.DenunciasPendientes
            };
        }

        public async Task<List<EstadisticaCategoria>> ObtenerPorCategoriaAsync()
        {
            var filas = await _http.Cliente.GetFromJsonAsync<List<EstadisticaCategoriaDto>>(
                "rest/v1/vista_estadisticas_categorias?select=categoria,ofertas_activas,oferentes");

            return (filas ?? new List<EstadisticaCategoriaDto>())
                .Select(f => new EstadisticaCategoria
                {
                    Categoria = f.Categoria,
                    OfertasActivas = f.OfertasActivas,
                    Oferentes = f.Oferentes
                })
                .ToList();
        }
    }
}
