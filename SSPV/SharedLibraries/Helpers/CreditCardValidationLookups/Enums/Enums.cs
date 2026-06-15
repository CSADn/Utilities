using System.ComponentModel;

#region Enumerados

public enum TipoTarjetaCredito
{

    [Description("Visa")]
    Visa = 201,

    [Description("American Express")]
    AmericanExpress = 202,

    [Description("Mastercard")]
    Mastercard = 203,

    [Description("Tarjeta Naranja")]
    TarjetaNaranja = 204,

    [Description("CMR - Falabella - Classic")]
    FalaClassic = 205,

    [Description("CMR - Falabella - Mastercard")]
    FalaMaster = 242,

    [Description("Cabal")]
    Cabal = 206,

    [Description("Cliper")]
    Cliper = 207,

    [Description("Tarjeta Nativa Visa")]
    NativaV = 208,

    [Description("Tarjeta Nativa Mastercard")]
    NativaM = 209,

    //TODO: Establecer nro en lookup antes
    [Description("Diners Club")]
    DinersClub = 230,
    [Description("Provencred")]
    Proven = 231,
    [Description("Enroute")]
    Enroute = 232,
    [Description("JCB")]
    Jcb = 233,
    [Description("Carta Automática")]
    Carta = 234,
    [Description("Elebar")]
    Elebar = 235,
    [Description("Cencosud")]
    Cencosud = 236,
    [Description("Discover")]
    Discover = 237,
    [Description("Credencial")]
    Credencial = 238,
    [Description("Tarjeta Mas")]
    Mas = 239,
    [Description("Tarjeta Italcred")]
    Italcred = 240,
    [Description("Tarjeta Nevada")]
    Nevada = 241,
    [Description("Favacard")]
    Favacard = 247,
    [Description("Usina")]
    Usina = 243,
    [Description("Garbarino")]
    Garbarino = 244,
    [Description("Carrefour")]
    Carrefour = 245,
    [Description("Francés")]
    Frances = 246,

    [Description("Tarjeta no válida")]
    InvalidCard = -1
}

public enum CardType
{
    [Description("UNKNOW")]
    Desconocido,
    [Description("CREDIT")]
    Credito,
    [Description("DEBIT")]
    Debito
}

#endregion