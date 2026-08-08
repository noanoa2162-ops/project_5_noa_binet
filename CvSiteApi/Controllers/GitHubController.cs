using Microsoft.AspNetCore.Mvc;
using CvSiteApi.Services;
using CvSiteApi.Services.Models;

namespace CvSiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        private readonly IGitHubService _gitHubService;
        private readonly ILogger<GitHubController> _logger;

        public GitHubController(
            IGitHubService gitHubService,
            ILogger<GitHubController> logger)
        {
            _gitHubService = gitHubService;
            _logger = logger;
        }

        [HttpGet("portfolio")]
        public async Task<ActionResult<IReadOnlyList<PortfolioRepositoryDto>>> GetPortfolio()
        {
            try
            {
                var portfolio = await _gitHubService.GetPortfolioAsync();
                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load the GitHub portfolio.");
                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "GitHub is temporarily unavailable.");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<SearchResultDto>>> SearchRepositories(
            [FromQuery] string? name = null,
            [FromQuery] string? language = null,
            [FromQuery] string? user = null)
        {
            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(language) &&
                string.IsNullOrWhiteSpace(user))
            {
                return BadRequest("Provide at least one search parameter: name, language, or user.");
            }

            try
            {
                var results = await _gitHubService.SearchRepositoriesAsync(name, language, user);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search GitHub repositories.");
                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "GitHub is temporarily unavailable.");
            }
        }
    }
}
