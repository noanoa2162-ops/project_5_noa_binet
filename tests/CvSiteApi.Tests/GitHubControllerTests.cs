using CvSiteApi.Controllers;
using CvSiteApi.Services;
using CvSiteApi.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CvSiteApi.Tests;

public class GitHubControllerTests
{
    [Fact]
    public async Task SearchRepositories_WithoutFilters_ReturnsBadRequest()
    {
        var controller = CreateController(new FakeGitHubService());

        var response = await controller.SearchRepositories();

        var result = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task GetPortfolio_ReturnsRepositories()
    {
        IReadOnlyList<PortfolioRepositoryDto> repositories =
        [
            new PortfolioRepositoryDto
            {
                Id = 1,
                Name = "portfolio-api",
                Language = "C#",
                Url = "https://github.com/example/portfolio-api"
            }
        ];

        var service = new FakeGitHubService
        {
            PortfolioHandler = () => Task.FromResult(repositories)
        };
        var controller = CreateController(service);

        var response = await controller.GetPortfolio();

        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(repositories, result.Value);
    }

    [Fact]
    public async Task SearchRepositories_WithFilter_ReturnsResults()
    {
        IReadOnlyList<SearchResultDto> repositories =
        [
            new SearchResultDto
            {
                Id = 2,
                Name = "aspnetcore",
                Owner = "dotnet",
                Language = "C#",
                Url = "https://github.com/dotnet/aspnetcore"
            }
        ];

        var service = new FakeGitHubService
        {
            SearchHandler = (_, _, _) => Task.FromResult(repositories)
        };
        var controller = CreateController(service);

        var response = await controller.SearchRepositories(language: "C#");

        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(repositories, result.Value);
    }

    [Fact]
    public async Task GetPortfolio_WhenGitHubFails_ReturnsGenericBadGateway()
    {
        var service = new FakeGitHubService
        {
            PortfolioHandler = () => Task.FromException<IReadOnlyList<PortfolioRepositoryDto>>(
                new InvalidOperationException("sensitive upstream detail"))
        };
        var controller = CreateController(service);

        var response = await controller.GetPortfolio();

        var result = Assert.IsType<ObjectResult>(response.Result);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status502BadGateway, result.StatusCode);
        Assert.Equal("GitHub is temporarily unavailable.", problem.Title);
        Assert.DoesNotContain("sensitive", problem.Title, StringComparison.OrdinalIgnoreCase);
    }

    private static GitHubController CreateController(IGitHubService service) =>
        new(service, NullLogger<GitHubController>.Instance);

    private sealed class FakeGitHubService : IGitHubService
    {
        public Func<Task<IReadOnlyList<PortfolioRepositoryDto>>> PortfolioHandler { get; init; } =
            () => Task.FromResult<IReadOnlyList<PortfolioRepositoryDto>>([]);

        public Func<string?, string?, string?, Task<IReadOnlyList<SearchResultDto>>> SearchHandler { get; init; } =
            (_, _, _) => Task.FromResult<IReadOnlyList<SearchResultDto>>([]);

        public Task<IReadOnlyList<PortfolioRepositoryDto>> GetPortfolioAsync() => PortfolioHandler();

        public Task<IReadOnlyList<SearchResultDto>> SearchRepositoriesAsync(
            string? repositoryName = null,
            string? language = null,
            string? userName = null) =>
            SearchHandler(repositoryName, language, userName);
    }
}
