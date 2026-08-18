using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IAiProductSuggestionService
{
    Task<AiProductSuggestion> SuggestAsync(AiProductSuggestionRequest request, CancellationToken cancellationToken = default);
}
