using System.ComponentModel;

namespace Common
{
    #region Enumerados

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
        Cabal = 36,

        [SISE(new string[] { "025", "125", "625", "725" }, "Tarjeta Naranja")]
        Naranja = 50,

        [SISE(new string[] { "077" }, "Tarjeta Cliper")]
        Cliper = 77,

        [SISE(new string[] { "401" }, "Tarjeta Nativa Visa")]
        NativaV = 401,

        [SISE(new string[] { "603" }, "Tarjeta Nativa Master")]
        NativaM = 603,

        [SISE(new string[] { "905" }, "Diners Club")]
        DinersClub = 905,

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

        [SISE(new string[] { "" }, "Tarjeta Nevada")]
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
        WS
    }

    public enum TipoUsuarioPerfil
    {
        Administrador = 1,
        Usuario = 2,
        CallCenter = 3
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
        [Linea("38", "COBERTURA COMPLEMENTARIA DE SALUD")]
        Salud,
        [Linea("46", "AUTOMOTORES - POLIZA FADEEAC")]
        AutomotoresFadeeac
    }

    public enum TipoEnvio
    {
        SalesForce,
        Teleperformance,
        SaleForce
    }

    #endregion
}

