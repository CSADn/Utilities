using LinqToExcel.Attributes;

namespace UnidosAfiliaciones.Application.Dtos
{
    public class PersonaExcelDto
    {
        public long Matricula { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Celular { get; set; }
        public string Email { get; set; }
        public string FechaNacimiento { get; set; }
        public string FechaAdhesion { get; set; }

        [ExcelColumn("Domicilio DNI")]
        public string DomicilioDni { get; set; }

        [ExcelColumn("Provincia DNI")]
        public string ProvinciaDni { get; set; }
        [ExcelColumn("Departamento DNI")]
        public string DepartamentoDni { get; set; }
        [ExcelColumn("Localidad DNI")]
        public string LocalidadDni { get; set; }
        [ExcelColumn("Nombre Censal DNI")]
        public string NombreCensalDni { get; set; }

        public string Provincia { get; set; }
        public string Departamento { get; set; }
        public string Localidad { get; set; }
        public string NombreCensal { get; set; }

        public string Estado { get; set; }
    }
}
