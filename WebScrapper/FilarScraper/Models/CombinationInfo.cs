using System.Text.Json;
using System.Text.Json.Serialization;

namespace FilarScraper.Models;

/// <summary>
/// Handles Odoo's quirk of returning <c>false</c> (boolean) instead of
/// <c>null</c> or <c>0</c> when an integer field has no value.
/// Deserializes <c>false</c>, <c>null</c>, or any non-numeric token as 0.
/// </summary>
public class OdooIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32(),
            // Odoo returns false instead of null/0 for missing int fields
            JsonTokenType.False  => 0,
            JsonTokenType.True   => 1,
            JsonTokenType.Null   => 0,
            // Strings like "39" — defensive fallback
            JsonTokenType.String => int.TryParse(reader.GetString(), out var v) ? v : 0,
            _                    => 0
        };
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}

public record JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("method")]
    public string Method { get; init; } = "call";

    [JsonPropertyName("id")]
    public int Id { get; init; } = 1;

    [JsonPropertyName("params")]
    public CombinationInfoParams Params { get; init; } = new();
}

public record CombinationInfoParams
{
    [JsonPropertyName("product_template_id")]
    public int ProductTemplateId { get; init; }

    [JsonPropertyName("product_id")]
    public int ProductId { get; init; } = 0;

    [JsonPropertyName("combination")]
    public int[] Combination { get; init; } = [];

    [JsonPropertyName("add_qty")]
    public int AddQty { get; init; } = 1;

    [JsonPropertyName("pricelist_id")]
    public bool PricelistId { get; init; } = false;
}

public record JsonRpcResponse<T>
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("result")]
    public T? Result { get; init; }

    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; init; }
}

public record JsonRpcError
{
    [JsonPropertyName("code")]
    public int Code { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}

public record CombinationInfoResult
{
    /// <summary>
    /// Odoo returns <c>false</c> (bool) instead of null/0 when the variant
    /// has no stock or is not available — handled by <see cref="OdooIntConverter"/>.
    /// </summary>
    [JsonPropertyName("product_id")]
    [JsonConverter(typeof(OdooIntConverter))]
    public int ProductId { get; init; }

    [JsonPropertyName("product_template_id")]
    public int ProductTemplateId { get; init; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("is_combination_possible")]
    public bool IsCombinationPossible { get; init; }

    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    [JsonPropertyName("list_price")]
    public decimal ListPrice { get; init; }

    [JsonPropertyName("carousel")]
    public string Carousel { get; init; } = string.Empty;
}
