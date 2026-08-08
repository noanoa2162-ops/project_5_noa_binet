using CvSiteApi.Services.Models;

namespace CvSiteApi.Services;

public interface IGitHubService
{
    Task<IReadOnlyList<PortfolioRepositoryDto>> GetPortfolioAsync();

    Task<IReadOnlyList<SearchResultDto>> SearchRepositoriesAsync(
        string? repositoryName = null,
        string? language = null,
        string? userName = null);
}
