namespace CvSiteApi.Services.Models
{
    /// <summary>
    /// DTO עבור Repository בפורטפוליו
    /// </summary>
    public class PortfolioRepositoryDto
    {
        /// <summary>
        /// ID ייחודי של ה-Repository ב-GitHub
        /// הוסף למידע נוסף - לא הוא חובה לפי הדרישות
        /// </summary>
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int Stars { get; set; }
        public int PullRequests { get; set; }
        public DateTime LastCommit { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
