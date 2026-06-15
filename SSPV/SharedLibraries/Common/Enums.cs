using System.ComponentModel;

namespace Common
{
    #region Enumerados

    /// <summary>
    /// Periodos Idem Lookups
    /// </summary>
    public enum PeriodosLookups
    {
        Mensual = 1,
        Semestral = 2,
        Anual = 3,
        Diario = 4
    }

    /// <summary>
    /// Medios de pago originales por aplicacion
    /// </summary>
    public enum MediosDePago
    {
        [SISE(new string[] { "000" }, "Pago en Efectivo")]
        Efectivo = 0,

        [SISE(new string[] { "000" }, "Pago Fácil")]
        PagoFacil = 4,

        [SISE(new string[] { "090" }, "Débito Directo")]
        DebitoDirecto = 5,

        [SISE(new string[] { "090" }, "Débito en Cuenta")]
        DebitoCuenta = 6,

        [SISE(new string[] { "090" }, "CBU")]
        CBU = 90,

        [SISE(new string[] { "901" }, "Tarjeta Visa")]
        Visa = 1,

        [SISE(new string[] { "902" }, "American Express")]
        Amex = 2,

        [SISE(new string[] { "903" }, "Tarjeta Mastercard")]
        Master = 3,

        [SISE(new string[] { "022", "422" }, "Tarjeta Falabella")]
        Falabella = 22,

        [SISE(new string[] { "936" }, "Tarjeta Cabal")]
        Cabal = 936,

        [SISE(new string[] { "025", "125", "625", "725" }, "Tarjeta Naranja")]
        Naranja = 50,

        [SISE(new string[] { "077" }, "Tarjeta Cliper")]
        Cliper = 77,

        [SISE(new string[] { "401" }, "Tarjeta Nativa Visa")]
        NativaV = 401,

        [SISE(new string[] { "603" }, "Tarjeta Nativa Master")]
        NativaM = 603,

        [SISE(new string[] { "905" }, "Diners Club")]
        DinersClub = 95, //905 era el correcto

        [SISE(new string[] { "041" }, "Provencred")]
        Provencred = 41,

        [SISE(new string[] { "" }, "Enroute", false)]
        Enroute,

        [SISE(new string[] { "" }, "JCB", false)]
        Jcb,

        [SISE(new string[] { "101" }, "Carta Automática")]
        Carta = 101,

        [SISE(new string[] { "078" }, "Elebar")]
        Elebar = 78,

        [SISE(new string[] { "703" }, "Cencosud")]
        Cencosud = 703,

        [SISE(new string[] { "" }, "Discover", false)]
        Discover,

        [SISE(new string[] { "020" }, "Credencial")]
        Credencial = 20,

        [SISE(new string[] { "503" }, "Tarjeta Mas")]
        Mas = 503,

        [SISE(new string[] { "102" }, "Tarjeta Italcred")]
        Italcred = 102,

        [SISE(new string[] { "80" }, "Tarjeta Nevada")]
        Nevada = 80,

        [SISE(new string[] { "" }, "Favacard", false)]
        Favacard,

        [SISE(new string[] { "115" }, "Usina")]
        Usina = 115,

        [SISE(new string[] { "705" }, "Garbarino")]
        Garbarino,

        [SISE(new string[] { "610" }, "Carrefour")]
        Carrefour = 610,

        [SISE(new string[] { "043" }, "Francés")]
        Frances = 43,

        [SISE(new string[] { "013" }, "SB CC")]
        SBCC = 13,

        [SISE(new string[] { "014" }, "SB CA")]
        SBCA = 14
    }

    /// <summary>
    /// Medios de pago idem lookups
    /// </summary>
    public enum MediosDePagoLookups
    {
        [SISE(new string[] { "EF" }, "Pago en Efectivo")]
        Efectivo = 1,

        [SISE(new string[] { "TA" }, "Tarjeta de Crédito")]
        TarjetaCredito = 2,

        [SISE(new string[] { "DD" }, "Débito Directo")]
        DebitoDirecto = 3,

        [SISE(new string[] { "BA" }, "CBU")]
        CBU = 4,

        [SISE(new string[] { "PR" }, "Pago Fácil")]
        PagoFacil = 5,

        [SISE(new string[] { "DC" }, "Débito en Cuenta")]
        DebitoCuenta = 6,

        [SISE(new string[] { "MP" }, "MercadoPago")]
        MercadoPago = 7
    }

    /// <summary>
    /// Conductos idem Lookups!
    /// </summary>
    public enum MediosPagoConductoLookups
    {
        //TODO: ver numeros correctos para algunos conductos
        [SISE(new string[] { "000" }, "Pago en Efectivo")]
        Efectivo = 101,

        [SISE(new string[] { "901" }, "Tarjeta Visa")]
        Visa = 201,

        [SISE(new string[] { "902" }, "American Express")]
        Amex = 202,

        [SISE(new string[] { "903" }, "Tarjeta Mastercard")]
        Mastercard = 203,

        [SISE(new string[] { "025", "125", "625", "725" }, "Tarjeta Naranja")]
        Naranja = 204,

        [SISE(new string[] { "022" }, "Tarjeta Falabella Classic")]
        FalabellaClassic = 205,

        [SISE(new string[] { "422" }, "Tarjeta Falabella Mastercard")]
        FalabellaMaster = 242,

        [SISE(new string[] { "936" }, "Tarjeta Cabal")]
        Cabal = 206,

        [SISE(new string[] { "077" }, "Tarjeta Cliper")]
        Cliper = 207,

        [SISE(new string[] { "401" }, "Tarjeta Nativa Visa")]
        NativaVisa = 208,

        [SISE(new string[] { "603" }, "Tarjeta Nativa Master")]
        NativaMastercard = 209,

        [SISE(new string[] { "090" }, "Débito Directo")]
        DebitoDirecto = 301,

        [SISE(new string[] { "090" }, "CBU")]
        CBU = 401,

        [SISE(new string[] { "000" }, "Pago Fácil")]
        PagoFacil = 501,

        [SISE(new string[] { "090" }, "Débito en Cuenta")]
        DebitoCuenta = 601,


        [SISE(new string[] { "905" }, "Diners Club")]
        DinersClub = 230,

        [SISE(new string[] { "041" }, "Provencred")]
        Provencred = 231,

        [SISE(new string[] { "" }, "Enroute", false)]
        Enroute = 232,

        [SISE(new string[] { "" }, "JCB", false)]
        Jcb = 233,

        [SISE(new string[] { "101" }, "Carta Automática")]
        Carta = 234,

        [SISE(new string[] { "078" }, "Elebar")]
        Elebar = 235,

        [SISE(new string[] { "703" }, "Cencosud")]
        Cencosud = 236,

        [SISE(new string[] { "" }, "Discover", false)]
        Discover = 237,

        [SISE(new string[] { "020" }, "Credencial")]
        Credencial = 238,

        [SISE(new string[] { "503" }, "Tarjeta Mas")]
        Mas = 239,

        [SISE(new string[] { "102" }, "Tarjeta Italcred")]
        Italcred = 240,

        [SISE(new string[] { "080" }, "Tarjeta Nevada")]
        Nevada = 241,

        [SISE(new string[] { "079" }, "Favacard", false)]
        Favacard = 242,

        [SISE(new string[] { "115" }, "Usina")]
        Usina = 243,

        [SISE(new string[] { "705" }, "Garbarino")]
        Garbarino = 244,

        [SISE(new string[] { "610" }, "Carrefour")]
        Carrefour = 245,

        [SISE(new string[] { "043" }, "Francés")]
        Frances = 246,

        [SISE(new string[] { "701" }, "MercadoPago")]
        MercadoPago = 701
    }

    public enum CondIva
    {
        ResponsableInscripto = 1,
        ResponsableNoInscripto = 2,
        Otro = 3,
        ConsumidorFinal = 4,
        Exento = 6,
        Monotributo = 9,
        NoCategorizado = 10,
        GranContribuyente = 99
    }

    public enum CondIIBB
    {
        ConvenioMultilateral = 1,
        Local = 2,
        NoInscriptoenIIBB = 3,
        Exento = 4
    }

    public enum TipoPersona
    {
        Fisica = 1,
        Juridica = 2
    }

    public enum TipoEstadoCivil
    {
        Casado = 1,
        Soltero = 2,
        SeparadoDivorciado = 4,
        Viudo = 5,
        NoInforma = 99
    }



    public enum Estado
    {
        [Description("A")]
        Alta = 1,

        [Description("B")]
        Baja = 2,

        [Description("M")]
        Modificacion = 3,

        [Description("S")]
        Si = 4,

        [Description("N")]
        No = 5,

        [Description("S")]
        SinEmitir = 6,

        [Description("E")]
        Emitida = 7,
    }

    public enum ProdOrg
    {
        [Description("P")]
        Productor = 1,
        [Description("O")]
        Organizador = 2,
    }

    public enum TipoUsuario
    {
        Normal,
        UsuarioUnico,
        WS,
        Sponsor = 6
    }

    public enum TipoUsuarioPerfil
    {
        Administrador = 1,
        Usuario = 2,
        CallCenter = 3,
        AdminCallCenter = 4
    }

    public enum TipoTarjeta
    {
        Credito,
        Debito
    }

    public enum TipoRendicion
    {
        MayorEntreEmisionVigencia = 1,
        PorFechaVigencia = 2,
        PorFechaEmision = 3
    }

    public enum TipoRedondeo
    {
        [Description("P")]
        PrimerDia,
        [Description("U")]
        UltimoDia
    }

    public enum TipoCron
    {
        Intervalos,
        Diario
    }

    public enum TipoDocumento
    {
        CI = 0,
        CUIT = 80,
        LE = 89,
        LC = 90,
        PAS = 94,
        DNI = 96,
        CUIL = 99
    }

    /// <summary>
    /// Lineas (Ramos)
    /// </summary>
    public enum Lineas
    {
        [Linea("01", "INCENCIO - (NO INCLUYE AUTOMOVILES)")]
        Incendio,
        [Linea("02", "TRANSPORTES")]
        Transportes,
        [Linea("04", "AUTOS PARTICULARES")]
        Autos,
        [Linea("05", "CRISTALES - (NO INCLUYE AUTOMOVILES)")]
        Cristales,
        [Linea("06", "ROBO - (NO INCLUYE AUTOMOVILES)")]
        Robo,
        [Linea("07", "RIESGOS VARIOS")]
        Plus,
        [Linea("08", "Integral RC")]
        IntegralRC,
        [Linea("09", "COMBINADO FAMILIAR")]
        Hogar,
        [Linea("11", "AUTOMOTORES COMERCIALES")]
        AutomotoresComerciales,
        [Linea("12", "ACCIDENTES PERSONALES")]
        AP,
        [Linea("14", "AUTOS COMERCIAL")]
        AutosComercial,
        [Linea("19", "SEGURO TECNICO")]
        SeguroTecnico,
        [Linea("20", "CALDERAS Y MAQUINARIA")]
        CalderasMaquinas,
        [Linea("21", "INTEGRAL CONSORCIO")]
        IntegralConsorcio,
        [Linea("22", "CASCOS")]
        Cascos,
        [Linea("24", "COMBINADO FAMILIAR/FIRE")]
        HogarIncendio,
        [Linea("25", "INTEGRAL COMERCIO")]
        IntegralComercio,
        [Linea("26", "SEGURO TECNICO (CONSTRUCCION/MONTAJE)")]
        SeguroTecnicoConstruccion,
        [Linea("27", "INCENDIO (OIL AND PETROL)")]
        IncendioOilPetrol,
        [Linea("28", "INCENCIO (CHEMICALS)")]
        IncendioChemicals,
        [Linea("29", "RIESGOS VARIOS")]
        Mapp,
        [Linea("38", "COBERTURA COMPLEMENTARIA DE SALUD")]
        Salud,
        [Linea("46", "AUTOMOTORES - POLIZA FADEEAC")]
        AutomotoresFadeeac,
        [Linea("48", "MOTOS")]
        Moto
    }

    public enum TipoEnvio
    {
        SalesForce,
        Teleperformance
    }

    public enum TipoCotizacion
    {
        [Description("Autos")]
        Autos = 1,
        [Description("Camiones")]
        Camiones = 2,
        [Description("Motos")]
        Motos = 3,
        [Description("Flota")]
        Flota = 4
    }


    #endregion
}

