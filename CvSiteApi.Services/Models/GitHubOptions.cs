namespace CvSiteApi.Services.Models
{
    public class GitHubOptions
    {
        public const string SectionName = "GitHub";

        public string UserName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
