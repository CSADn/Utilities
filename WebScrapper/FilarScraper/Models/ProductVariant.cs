namespace FilarScraper.Models;

public record ProductVariant
{
    public int ProductId { get; init; }
    public int AttributeValueId { get; init; }
    public string ColorName { get; init; } = string.Empty;
    public string HexColor { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public List<ProductImage> Images { get; init; } = [];
}

public record ProductImage
{
    public string Url { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ColorName { get; init; } = string.Empty;
    public bool Downloaded { get; set; }
    public string LocalPath { get; set; } = string.Empty;
}
