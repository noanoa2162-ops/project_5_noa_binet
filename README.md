# CV Site - GitHub Portfolio API

## 📋 תיאור הפרויקט

אפליקציית Web API המתחברת לחשבון GitHub ומציגה את תיק העבודות (פורטפוליו) של המפתח, כולל:

- **רשימת Repositories**: כל ה-repositories של המשתמש עם מידע מפורט
- **חיפוש ציבורי**: חיפוש ב-repositories ציבוריים של GitHub עם סינון
- **Caching חכם**: שמירת מידע בזיכרון עם בדיקת פעילות חדשה בGitHub

## 🛠️ טכנולוגיות

- **.NET 8.0** Web API (ASP.NET Core)
- **Octokit v14.0.0** - GitHub API Client
- **C#** - שפת הפיתוח
- **User Secrets** - שמירה בטוחה של Token

## 📁 מבנה הפרויקט

```
project_5_noa_binet/
├── CvSiteApi/                 # Web API Project
│   ├── Controllers/
│   │   └── GitHubController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── CvSiteApi.csproj
│
└── CvSiteApi.Services/        # Class Library (Services)
    ├── Models/
    │   ├── GitHubOptions.cs
    │   ├── PortfolioRepositoryDto.cs
    │   └── SearchResultDto.cs
    ├── GitHubService.cs
    └── CvSiteApi.Services.csproj
```

## 🚀 התחלה

### דרישות מקדימות
- .NET 8.0 SDK
- חשבון GitHub
- Personal Access Token מ-GitHub

### 1️⃣ יצירת GitHub Token

1. כנס ל-[GitHub Settings](https://github.com/settings)
2. בחר **Developer settings** → **Personal access tokens** → **Tokens (classic)**
3. לחץ על **Generate new token**
4. בחר את התאריכים וההרשאות (לפחות `repo`)
5. העתק את הטוקן

### 2️⃣ הגדרת User Secrets

הרץ את הפקודות הבאות מתיקייה `CvSiteApi`:

```powershell
# הגדרת שם המשתמש שלך ב-GitHub
dotnet user-secrets set "GitHub:UserName" "YOUR_USERNAME"

# הגדרת ה-Token שלך
dotnet user-secrets set "GitHub:Token" "YOUR_TOKEN"
```

**לדוגמה:**
```powershell
dotnet user-secrets set "GitHub:UserName" "YOUR_GITHUB_USERNAME"
dotnet user-secrets set "GitHub:Token" "YOUR_GITHUB_TOKEN"
```

⚠️ **הערה חשובה:** אל תשים את ה-Token שלך בGitHub! שמור אותו במחשבה שלך בלבד!

### 3️⃣ הרצת הפרויקט

```powershell
# מתיקייה CvSiteApi
dotnet build
dotnet run
```

השרת יתחיל על: **http://localhost:5170**

## 📡 API Endpoints

### 1. Get Portfolio (הריפוזיטוריז שלך)

```http
GET /api/github/portfolio
```

**תשובה (200 OK):**
```json
[
  {
    "id": 1091259897,
    "name": "elementrix",
    "language": "TypeScript",
    "stars": 0,
    "pullRequests": 0,
    "lastCommit": "2025-11-06T19:11:14",
    "url": "https://github.com/noanoa2162-ops/elementrix",
    "description": null
  }
]
```

**מה שהוא עושה:**
- ✅ מביא את כל ה-repositories שלך
- ✅ עבור כל repo: שפה, כוכבים, PRs, commit אחרון, קישור
- ✅ **Caching חכם**: שומר את הנתונים 5 דקות
- ✅ **Challenge**: בודק אם יש פעילות חדשה בGitHub:
  - אם יש פעילות → מחק את ה-Cache ושלוף מחדש
  - אם אין → החזר מה-Cache

---

### 2. Search Repositories (חיפוש ציבורי)

```http
GET /api/github/search?name=clean&language=C%23
```

**פרמטרים (כולם אופציונליים):**
- `name` - שם Repository (לדוגמה: "clean")
- `language` - שפת פיתוח (לדוגמה: "C#", "JavaScript", "Python")
- `user` - שם משתמש (לדוגמה: "microsoft")

**דוגמאות:**
```
GET /api/github/search?name=clean&language=C%23
GET /api/github/search?language=JavaScript
GET /api/github/search?user=microsoft
GET /api/github/search?name=react&language=JavaScript&user=facebook
```

**תשובה (200 OK):**
```json
[
  {
    "id": 114588511,
    "name": "clean.net",
    "owner": "malbruk",
    "url": "https://github.com/malbruk/clean.NET",
    "description": null,
    "language": "C#",
    "stars": 0,
    "forks": 0
  }
]
```

**מה שהוא עושה:**
- ✅ חיפוש ציבורי ב-GitHub (ללא הזדהות)
- ✅ סינון לפי שם, שפה, משתמש
- ✅ החזרת תוצאות עם כל המידע

---

## 🧪 בדיקת ה-API

### דרך Swagger UI

1. כנס ל-**http://localhost:5170/swagger**
2. בחר את ה-endpoint שתרצה
3. לחץ **Try it out** → **Execute**

### דוגמה בcURL

```bash
# Get Portfolio
curl -X GET "http://localhost:5170/api/github/portfolio" \
  -H "accept: application/json"

# Search
curl -X GET "http://localhost:5170/api/github/search?name=clean&language=C%23" \
  -H "accept: application/json"
```

---

## 🎯 תכונות מיוחדות

### 💾 Caching עם Activity Detection (Challenge)

במקום לנקות את ה-Cache כל X דקות, הפרויקט:

1. **בודק אם יש פעילות חדשה** ב-GitHub של המשתמש
2. **אם יש פעילות:**
   - מחק את ה-Cache
   - שלוף מחדש מ-GitHub
   - החזר נתונים טריים

3. **אם אין פעילות:**
   - החזר מה-Cache (דיוק 5 דקות)
   - חסוך בקריאות ל-API

**קוד:**
```csharp
if (await HasNewActivityAsync())
{
    _cachedPortfolio.Clear();
    _lastCacheTime = DateTime.MinValue;
}
```

---

## 🏗️ ארכיטקטורה

### 3-Tier Architecture

```
Controllers (HTTP Layer)
    ↓
Services (Business Logic)
    ↓
Models (Data Transfer Objects)
```

### Dependency Injection

כל המחלקות מתחבליות דרך Program.cs:

```csharp
builder.Services.Configure<GitHubOptions>(
    builder.Configuration.GetSection("GitHub"));
builder.Services.AddSingleton<GitHubService>();
```

---

## 📝 קבצים חשובים

| קובץ | תיאור |
|------|-------|
| `GitHubService.cs` | הלוגיקה הראשית - התחברות ל-GitHub, Caching, Activity Detection |
| `GitHubController.cs` | ה-Endpoints של ה-API |
| `Program.cs` | Configuration ו-Dependency Injection |
| `GitHubOptions.cs` | Container לקונפיגורציה (UserName, Token) |
| `PortfolioRepositoryDto.cs` | DTO עבור repositories של המשתמש |
| `SearchResultDto.cs` | DTO עבור תוצאות החיפוש |

---

## ⚙️ Configuration

### appsettings.json

```json
{
  "GitHub": {
    "UserName": "noanoa2162-ops",
    "Token": "YOUR_TOKEN"
  }
}
```

**⚠️ אל תשים את הטוקן בקוד! שתמש ב-User Secrets**

---

## 🐛 Troubleshooting

### "User not found"
```
Error: users/noanoa2162/repos was not found
```
**פתרון:** בדוק שה-Username נכון ב-User Secrets

### "Bad credentials"
```
Error: Bad credentials
```
**פתרון:** בדוק שה-Token תקף ול-expired

### Port 5170 is already in use
```powershell
Get-Process dotnet | Stop-Process -Force
```

---

## 📚 References

- [Octokit Documentation](https://octokitnet.readthedocs.io/)
- [GitHub API Docs](https://docs.github.com/en/rest)
- [User Secrets in .NET](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration)

---

## 📄 License

Project 5 - Educational Purpose

---

**אדם שכתב:** Noa  
**תאריך:** December 2025  
**Status:** ✅ Completed
