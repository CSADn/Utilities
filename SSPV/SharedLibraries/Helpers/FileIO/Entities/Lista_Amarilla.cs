using FileHelpers;
using System;

namespace EntitiesIO
{
    [DelimitedRecord(";")]
    [IgnoreFirst(1)]
    public class Lista_Amarilla
    {
        #region Propiedades

        [FieldConverter(ConverterKind.DateMultiFormat, "yyyy-MM-dd", "dd/MM/yyyy", "yyyy-MM-dd")]
        public DateTime FechaAlta;

        public string Dni;

        public string CuitCuil;

        public string Patente;

        public string NroChasis;

        public string NroMotor;

        public int? IdTipoAlerta;

        [FieldConverter(ConverterKind.DateMultiFormat, "yyyyMMdd", "dd/MM/yyyy", "yyyy-MM-dd")]
        public DateTime? VigenciaHasta;

        #endregion

    }
}
