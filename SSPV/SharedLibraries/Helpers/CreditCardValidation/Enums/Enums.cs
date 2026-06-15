using System.ComponentModel;

#region Enumerados

public enum TipoTarjetaCredito
{
    [Description("Visa")]
    Visa = 1,
    [Description("American Express")]
    AmericanExpress = 2,
    [Description("Mastercard")]
    Mastercard = 3,
    [Description("Tarjeta Naranja")]
    TarjetaNaranja = 50,
    [Description("Diners Club")]
    DinersClub = 95, //905 era el correcto
    [Description("Provencred")]
    Proven = 41,
    [Description("Enroute")]
    Enroute,
    [Description("JCB")]
    Jcb,
    [Description("Tarjeta Nativa Visa")]
    NativaV = 401,
    [Description("Tarjeta Nativa Mstercard")]
    NativaM = 603,
    [Description("Carta Automática")]
    Carta = 101,
    [Description("Elebar")]
    Elebar = 78,
    [Description("CMR - Falabella")]
    Fala = 22,
    [Description("Cencosud")]
    Cencosud = 703,
    [Description("Argencard")]
    Argencard,
    [Description("Cabal")]
    Cabal = 936,
    [Description("Discover")]
    Discover,
    [Description("Credencial")]
    Credencial = 20,
    [Description("Tarjeta Mas")]
    Mas = 503,
    [Description("Tarjeta Italcred")]
    Italcred = 102,
    [Description("Tarjeta Nevada")]
    Nevada = 80,
    [Description("Favacard")]
    Favacard,
    [Description("Cliper")]
    Cliper = 77,
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