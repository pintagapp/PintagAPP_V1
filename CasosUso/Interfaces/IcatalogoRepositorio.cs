using PintagAPP.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace PintagAPP.CasosUso.Interfaces
{
    public interface ICatalogoRepositorio
    {
        Task<List<Categoria>> ObtenerCategoriasAsync();
    }
}
