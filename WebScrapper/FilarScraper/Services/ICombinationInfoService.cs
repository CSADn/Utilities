using FilarScraper.Models;

namespace FilarScraper.Services;

public interface ICombinationInfoService
{
    /// <summary>
    /// Calls Odoo's /website_sale/get_combination_info for a given variant
    /// and returns the resolved product images extracted from the carousel HTML.
    /// </summary>
    Task<(int ProductId, List<ProductImage> Images)> GetVariantImagesAsync(
        int productTemplateId,
        int attributeValueId,
        string colorName,
        CancellationToken cancellationToken = default);
}
