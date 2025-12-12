namespace CvSiteApi.Services.Models
{
    /// <summary>
    /// DTO עבור תוצאות חיפוש ציבורי ב-GitHub
    /// </summary>
    public class SearchResultDto
    {
        /// <summary>
        /// ID ייחודי של ה-Repository ב-GitHub
        /// הוסף למידע נוסף - לא הוא חובה לפי הדרישות
        /// </summary>
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Language { get; set; } = string.Empty;
        public int Stars { get; set; }
        public int Forks { get; set; }
    }
}
