#region Clases

/// <summary>
/// Clase para la deserializacion de la tarjeta
/// </summary>
internal class TarjetaJson
{
    public string bin { get; set; }

    public string brand { get; set; }

    public string sub_brand { get; set; }

    public string country_code { get; set; }

    public string country_name { get; set; }

    public string bank { get; set; }

    public string card_type { get; set; }

    public string card_category { get; set; }

    public int latitude { get; set; }

    public int longitude { get; set; }

    public string query_time { get; set; }
}

#endregion