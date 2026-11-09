using System;
using System.Collections.Generic;
using System.Text;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface IContactoRepositorio
    {
        Task RegistrarAsync(Guid usuarioId, long perfilId, string canal);

        Task<List<long>> ObtenerPerfilesContactadosAsync(Guid usuarioId);
    }
}
