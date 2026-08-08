using Octokit;
using Microsoft.Extensions.Options;
using CvSiteApi.Services.Models;

namespace CvSiteApi.Services
{
    public class GitHubService : IGitHubService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private readonly GitHubClient _client;
        private readonly GitHubOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly SemaphoreSlim _portfolioLock = new(1, 1);

        private List<PortfolioRepositoryDto> _cachedPortfolio = new();
        private DateTimeOffset _lastCacheTime = DateTimeOffset.MinValue;
        private DateTimeOffset _lastActivityCheckTime = DateTimeOffset.MinValue;

        public GitHubService(
            IOptions<GitHubOptions> options,
            TimeProvider timeProvider)
        {
            _options = options.Value;
            _timeProvider = timeProvider;

            if (string.IsNullOrWhiteSpace(_options.UserName))
            {
                throw new InvalidOperationException("GitHub:UserName is required.");
            }

            _client = new GitHubClient(new ProductHeaderValue("CvSiteApi"));

            if (!string.IsNullOrWhiteSpace(_options.Token))
            {
                _client.Credentials = new Credentials(_options.Token);
            }
        }

        public async Task<IReadOnlyList<PortfolioRepositoryDto>> GetPortfolioAsync()
        {
            await _portfolioLock.WaitAsync();

            try
            {
                var now = _timeProvider.GetUtcNow();
                var cacheIsFresh = _cachedPortfolio.Count > 0 &&
                    now - _lastCacheTime < CacheDuration;

                if (cacheIsFresh && !await HasNewActivityAsync())
                {
                    return _cachedPortfolio.AsReadOnly();
                }

                var repositories = await _client.Repository.GetAllForUser(_options.UserName);
                var portfolioList = new List<PortfolioRepositoryDto>();

                foreach (var repo in repositories)
                {
                    try
                    {
                        var portfolioItem = await BuildPortfolioItemAsync(repo);
                        portfolioList.Add(portfolioItem);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing repository {repo.Name}: {ex.Message}");
                    }
                }

                _cachedPortfolio = portfolioList;
                _lastCacheTime = now;
                _lastActivityCheckTime = now;

                return _cachedPortfolio.AsReadOnly();
            }
            catch (Exception ex)
            {
                if (_cachedPortfolio.Count > 0)
                {
                    return _cachedPortfolio.AsReadOnly();
                }

                throw new InvalidOperationException("Unable to fetch the GitHub portfolio.", ex);
            }
            finally
            {
                _portfolioLock.Release();
            }
        }

        public async Task<IReadOnlyList<SearchResultDto>> SearchRepositoriesAsync(
            string? repositoryName = null,
            string? language = null,
            string? userName = null)
        {
            try
            {
                var searchTerm = "";

                if (!string.IsNullOrWhiteSpace(repositoryName))
                {
                    searchTerm = repositoryName;
                }

                if (!string.IsNullOrWhiteSpace(language))
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                        searchTerm += " ";
                    searchTerm += $"language:{language}";
                }

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                        searchTerm += " ";
                    searchTerm += $"user:{userName}";
                }

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = "stars:>0";
                }

                var searchRequest = new SearchRepositoriesRequest(searchTerm);
                var result = await _client.Search.SearchRepo(searchRequest);

                var searchResults = result.Items.Select(repo => new SearchResultDto
                {
                    Id = repo.Id,
                    Name = repo.Name,
                    Owner = repo.Owner.Login,
                    Url = repo.HtmlUrl,
                    Description = repo.Description,
                    Language = repo.Language ?? "Unknown",
                    Stars = repo.StargazersCount,
                    Forks = repo.ForksCount
                }).ToList();

                return searchResults;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to search GitHub repositories.", ex);
            }
        }

        private async Task<PortfolioRepositoryDto> BuildPortfolioItemAsync(Repository repo)
        {
            var portfolioItem = new PortfolioRepositoryDto
            {
                Id = repo.Id,
                Name = repo.Name,
                Stars = repo.StargazersCount,
                Url = repo.HtmlUrl,
                Description = repo.Description,
                Language = repo.Language ?? "Unknown"
            };

            try
            {
                var commits = await _client.Repository.Commit.GetAll(
                    repo.Owner.Login,
                    repo.Name,
                    new ApiOptions { PageSize = 1, PageCount = 1 });

                if (commits.Count > 0)
                {
                    portfolioItem.LastCommit = commits[0].Commit.Author.Date.DateTime;
                }
            }
            catch
            {
                portfolioItem.LastCommit = repo.PushedAt?.DateTime ?? DateTime.MinValue;
            }

            try
            {
                var request = new PullRequestRequest { State = ItemStateFilter.All };
                var prs = await _client.PullRequest.GetAllForRepository(
                    repo.Owner.Login,
                    repo.Name,
                    request);
                portfolioItem.PullRequests = prs.Count;
            }
            catch
            {
                portfolioItem.PullRequests = 0;
            }

            return portfolioItem;
        }

        private async Task<bool> HasNewActivityAsync()
        {
            try
            {
                var events = await _client.Activity.Events.GetAllUserPerformed(
                    _options.UserName,
                    new ApiOptions { PageSize = 10, PageCount = 1 });

                if (events.Count == 0)
                {
                    return false;
                }

                var latestEvent = events.FirstOrDefault();
                return latestEvent is not null &&
                    latestEvent.CreatedAt > _lastActivityCheckTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking activity: {ex.Message}");
                return false;
            }
        }
    }
}
