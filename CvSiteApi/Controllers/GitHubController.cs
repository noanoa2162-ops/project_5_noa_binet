using Microsoft.AspNetCore.Mvc;
using CvSiteApi.Services;
using CvSiteApi.Services.Models;

namespace CvSiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        private readonly GitHubService _gitHubService;

        /// <summary>
        /// Constructor - מקבל את GitHubService דרך Dependency Injection
        /// .NET אוטומטית מזריקה אותו בשביל אנו
        /// </summary>
        public GitHubController(GitHubService gitHubService)
        {
            _gitHubService = gitHubService;
        }

        /// <summary>
        /// GET /api/github/portfolio
        /// מחזיר את כל הריפוזיטוריז שלך עם מידע מלא
        /// </summary>
        [HttpGet("portfolio")]
        public async Task<ActionResult<List<PortfolioRepositoryDto>>> GetPortfolio()
        {
            try
            {
                var portfolio = await _gitHubService.GetPortfolioAsync();

                if (portfolio == null || portfolio.Count == 0)
                {
                    return NotFound("No repositories found");
                }

                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/github/search
        /// חיפוש ציבורי ב-GitHub
        /// פרמטרים אופציונליים:
        /// - name: שם Repository (לדוגמה: "angular")
        /// - language: שפת פיתוח (לדוגמה: "CSharp", "JavaScript")
        /// - user: שם משתמש (לדוגמה: "microsoft")
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<SearchResultDto>>> SearchRepositories(
            [FromQuery] string? name = null,
            [FromQuery] string? language = null,
            [FromQuery] string? user = null)
        {
            try
            {
                // בדוק שלפחות פרמטר אחד נשלח
                if (string.IsNullOrWhiteSpace(name) && 
                    string.IsNullOrWhiteSpace(language) && 
                    string.IsNullOrWhiteSpace(user))
                {
                    return BadRequest("Please provide at least one search parameter (name, language, or user)");
                }

                var results = await _gitHubService.SearchRepositoriesAsync(name, language, user);

                if (results == null || results.Count == 0)
                {
                    return NotFound("No repositories found matching your criteria");
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
