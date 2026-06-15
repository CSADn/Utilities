using System.Collections.Generic;

namespace UnidosAfiliaciones.Application.Dtos
{
    public class DatatableDto<T> where T : class
    {
        public int? Draw { get; set; }
        public long RecordsFiltered { get; set; }
        public long RecordsTotal { get; set; }
        public int UserRole { get; set; }

        public IList<T> Data { get; set; }
    }
}
