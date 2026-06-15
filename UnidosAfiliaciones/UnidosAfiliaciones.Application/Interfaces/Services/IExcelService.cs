using System.Collections.Generic;
using UnidosAfiliaciones.Application.Dtos;

namespace UnidosAfiliaciones.Application.Interfaces.Services
{
    public interface IExcelService
    {
        IList<PersonaExcelDto> DownloadAndFetch();
        IList<PersonaExcelDto> FetchData(string filename);
    }
}