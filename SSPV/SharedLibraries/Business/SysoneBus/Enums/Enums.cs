using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sysoneBus.Enums
{

    /// <summary>
    /// Lineas o Ramos
    /// </summary>
    public enum Lineas
    {

        [Linea(14000, "ACCIDENTES PERSONALES")]
        AP,
        [Linea(100, "AUTOS COMERCIAL")]
        AutosComercial,
        [Linea(200, "SEGURO TECNICO")]
        SeguroTecnico,
        [Linea(300, "CALDERAS Y MAQUINARIA")]
        CalderasMaquinas,
        [Linea(400, "INTEGRAL CONSORCIO")]
        IntegralConsorcio,
        [Linea(500, "CASCOS")]
        Cascos,
        [Linea(600, "COMBINADO FAMILIAR/FIRE")]
        HogarIncendio,
        [Linea(700, "INTEGRAL COMERCIO")]
        IntegralComercio,
        [Linea(800, "SEGURO TECNICO (CONSTRUCCION/MONTAJE)")]
        SeguroTecnicoConstruccion,
        [Linea(900, "INCENDIO (OIL AND PETROL)")]
        IncendioOilPetrol,
        [Linea(1000, "INCENCIO (CHEMICALS)")]
        IncendioChemicals,
        [Linea(1100, "RIESGOS VARIOS")]
        Mapp,
        [Linea(1200, "COBERTURA COMPLEMENTARIA DE SALUD")]
        Salud,
        [Linea(1300, "AUTOMOTORES - POLIZA FADEEAC")]
        AutomotoresFadeeac

    }
    /// <summary>
    /// Tipo de Docuemntos
    /// </summary>
    public enum TipoDocumento
    {
        [TipoDocumento(1,"Docomuento Nacional de Identidad")]
        DNI,
        [TipoDocumento(2, "C.U.I.T.")]
        CUIT,
        [TipoDocumento(3, "Libreta Civica")]
        LC
        
    }
    /// <summary>
    /// Tipo de Persona
    /// </summary>
    public enum TipoPersona
    {
        [TipoPersona(1,"Fisica")]
        FISICA,
        [TipoPersona(2, "Juridica")]
        JURIDICA
    }




}
