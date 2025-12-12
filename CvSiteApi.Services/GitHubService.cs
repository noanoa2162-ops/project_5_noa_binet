using Octokit;
using Microsoft.Extensions.Options;
using CvSiteApi.Services.Models;

namespace CvSiteApi.Services
{
    public class GitHubService
    {
        private readonly GitHubClient _client;
        private readonly GitHubOptions _options;

        // Cache - שומר את הנתונים בזיכרון כדי שלא נביא מ-GitHub כל פעם
        private List<PortfolioRepositoryDto> _cachedPortfolio = new();
        private DateTime _lastCacheTime = DateTime.MinValue;
        private DateTime _lastActivityCheckTime = DateTime.MinValue;

        /// <summary>
        /// Constructor - משדרג את GitHubClient עם הטוקן שלך
        /// </summary>
        public GitHubService(IOptions<GitHubOptions> options)
        {
            _options = options.Value;

            // יוצר GitHubClient שמדבר עם GitHub
            _client = new GitHubClient(new ProductHeaderValue("CvSiteApi"));

            // אם יש טוקן - נוסיף אותו להזדהות (לגישה למידע פרטי)
            if (!string.IsNullOrWhiteSpace(_options.Token))
            {
                _client.Credentials = new Credentials(_options.Token);
            }
        }

        /// <summary>
        /// GetPortfolioAsync - מביא את רשימת כל הריפוזיטוריז שלך עם מידע מלא
        /// זו הפונקציה המרכזית בפרויקט!
        /// </summary>
        public async Task<List<PortfolioRepositoryDto>> GetPortfolioAsync()
        {
            try
            {
                // בדוק אם צריך לרענן את ה-Cache
                // אם היתה פעילות חדשה ב-GitHub - מחק את ה-Cache
                if (await HasNewActivityAsync())
                {
                    _cachedPortfolio.Clear();
                    _lastCacheTime = DateTime.MinValue;
                }

                // אם יש נתונים ב-Cache ועדיין טרים - החזר אותם
                if (_cachedPortfolio.Count > 0 && DateTime.Now.Subtract(_lastCacheTime).TotalSeconds < 300)
                {
                    return _cachedPortfolio;
                }

                // אחרת - שלוף מ-GitHub
                var repositories = await _client.Repository.GetAllForUser(_options.UserName);
                var portfolioList = new List<PortfolioRepositoryDto>();

                foreach (var repo in repositories)
                {
                    try
                    {
                        // בנה את ה-DTO עם המידע של כל Repository
                        var portfolioItem = await BuildPortfolioItemAsync(repo);
                        portfolioList.Add(portfolioItem);
                    }
                    catch (Exception ex)
                    {
                        // אם משהו נכשל עבור repo אחד - המשך לשאר
                        Console.WriteLine($"Error processing repository {repo.Name}: {ex.Message}");
                    }
                }

                // שמור ב-Cache
                _cachedPortfolio = portfolioList;
                _lastCacheTime = DateTime.Now;
                _lastActivityCheckTime = DateTime.Now;

                return portfolioList;
            }
            catch (Exception ex)
            {
                // אם יש שגיאה - כדי להחזיר Cache ישן אם יש
                if (_cachedPortfolio.Count > 0)
                {
                    return _cachedPortfolio;
                }
                throw new Exception($"Error fetching portfolio: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// SearchRepositoriesAsync - חיפוש ציבורי ב-GitHub לפי קריטריונים
        /// </summary>
        public async Task<List<SearchResultDto>> SearchRepositoriesAsync(
            string? repositoryName = null,
            string? language = null,
            string? userName = null)
        {
            try
            {
                // בנה את הבקשה
                var searchTerm = "";

                // בנה את המונח לחיפוש
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

                // אם לא בחרו כלום - תביא את המלא
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = "stars:>0";
                }

                var searchRequest = new SearchRepositoriesRequest(searchTerm);

                // שלח ל-GitHub וקבל תוצאות
                var result = await _client.Search.SearchRepo(searchRequest);

                // המירו ל-DTO - רק השדות שצריכים
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
                throw new Exception($"Error searching repositories: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// BuildPortfolioItemAsync - פונקציה פרטית שבונה DTO אחד עם כל המידע
        /// </summary>
        private async Task<PortfolioRepositoryDto> BuildPortfolioItemAsync(Repository repo)
        {
            // קחי את המידע שיש ישירות ב-Repository
            var portfolioItem = new PortfolioRepositoryDto
            {
                Id = repo.Id,
                Name = repo.Name,
                Stars = repo.StargazersCount,
                Url = repo.HtmlUrl,
                Description = repo.Description,
                Language = repo.Language ?? "Unknown"
            };

            // שלוף את ה-Commit האחרון
            try
            {
                var commits = await _client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name, new ApiOptions { PageSize = 1 });
                if (commits.Count > 0)
                {
                    portfolioItem.LastCommit = commits[0].Commit.Author.Date.DateTime;
                }
            }
            catch
            {
                // אם יש בעיה - תשים ערך ברירת מחדל
                portfolioItem.LastCommit = repo.PushedAt?.DateTime ?? DateTime.MinValue;
            }

            // שלוף את מספר ה-Pull Requests
            try
            {
                var prs = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
                portfolioItem.PullRequests = prs.Count;
            }
            catch
            {
                portfolioItem.PullRequests = 0;
            }

            return portfolioItem;
        }

        /// <summary>
        /// HasNewActivityAsync - בודק אם היתה פעילות חדשה בחשבון שלך ב-GitHub
        /// זה ה-האתגר! בודק פעולות אמיתיות בלי לחכות X דקות
        /// </summary>
        private async Task<bool> HasNewActivityAsync()
        {
            try
            {
                // קח את ה-10 אירועים האחרונים של המשתמש
                var events = await _client.Activity.Events.GetAllUserPerformed(_options.UserName, new ApiOptions { PageSize = 10 });

                if (events.Count == 0)
                {
                    return false;
                }

                // קח את אירוע המוקדם ביותר (החדש ביותר)
                var latestEvent = events.FirstOrDefault();

                if (latestEvent == null)
                {
                    return false;
                }

                // בדוק אם האירוע החדש יותר מאחרון פעם שבדקנו
                if (latestEvent.CreatedAt > _lastActivityCheckTime)
                {
                    return true; // יש פעילות חדשה!
                }

                return false; // אין פעילות חדשה
            }
            catch (Exception ex)
            {
                // אם יש שגיאה בבדיקה - אל תשבור - פשוט החזר false
                Console.WriteLine($"Error checking activity: {ex.Message}");
                return false;
            }
        }
    }
}
