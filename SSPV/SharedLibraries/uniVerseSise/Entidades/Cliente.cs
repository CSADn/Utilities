using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades
{
    /// <summary>
    /// Almacenamiento Asegurados
    /// MASEG
    /// </summary>
    [NombreArchivo(Archivo = "MASEG")]
    public class Cliente
    {
        [DatoCampo(Id = true)]
        public string Id { get; set; }

        //MASEG(Asegurados)  Apellido MASEG<101>
        [DatoCampo(101)]
        public string Apellido { get; set; }

        //MASEG (Asegurados)  Nombre MASEG<102>
        [DatoCampo(102)]
        public string Nombre { get; set; }

        //MASEG (Asegurados)  Tipo Documento  MASEG<37>
        [DatoCampo(37)]
        public int? TipoDocumento { get; set; }

        //MASEG(Asegurados)  Nro.Documento MASEG<38>
        [DatoCampo(38)]
        public long? NroDocumento { get; set; }

        //MASEG (Asegurados)  CUIT MASEG<35>
        [DatoCampo(35)]
        public long? CUIT { get; set; }

        //MASEG (Asegurados)  Tipo Persona    MASEG<54>	1 = Fisica, 2 = Juridica
        [DatoCampo(54)]
        public int? TipoPersona { get; set; }

        //MASEG(Asegurados)  Domicilio - Calle MASEG<3>
        [DatoCampo(3)]
        public string DomicilioCalle { get; set; }

        //MASEG (Asegurados)  Domicilio - Número MASEG<4>
        [DatoCampo(4)]
        public string DomicilioNumero { get; set; }

        //MASEG (Asegurados)  Domicilio - Código Postal   MASEG<7>
        [DatoCampo(7)]
        public string DomicilioCodigoPostal { get; set; }

        //MASEG(Asegurados)  Domicilio - Localidad MASEG<8>
        [DatoCampo(8)]
        public string DomicilioLocalidad { get; set; }

        //MASEG (Asegurados)  Domicilio - Provincia MASEG<9>    FK a TPCIA
        [DatoCampo(9)]
        public int? IdProvincia { get; set; }

        //MASEG (Asegurados)  Nro.IIBB MASEG<84>
        [DatoCampo(84)]
        public string NroIIBB { get; set; }

        //MASEG (Asegurados)  Tipo IVA    MASEG<36> Se adjunta comentario
        //"1"  "Resp.Inscripto"
        //"2"  "Resp. No Inscripto"
        //"3"  "No responsable"
        //"4"  "Consumidor Final" 
        //"5"  "Resp. Inscripto GC"
        //"6"  "Exento" 
        //"7"  "Insc. Exen.Percep." 
        //"9"  "Monotributista" 
        //"10" "No Categorizado"
        [DatoCampo(36)]
        public int? TipoIVA { get; set; }
    }
}
