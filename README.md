# CV Site – GitHub Portfolio API

## 📋 תיאור הפרויקט
אפליקציית **Web API** המתחברת ל־GitHub ומציגה תיק עבודות (Portfolio) של מפתח באמצעות Octokit.  
המערכת מספקת:
- הצגת ריפוזיטוריז אישיים עם מידע מורחב  
- חיפוש ריפוזיטוריז ציבוריים  
- Caching חכם לשיפור ביצועים  
- איתור פעילות חדשה ב־GitHub לרענון נתונים  

---

## 🛠️ טכנולוגיות
- .NET 8 – ASP.NET Core Web API  
- C#  
- Octokit v14  
- User Secrets  
- Dependency Injection  

---

## 📁 מבנה הפרויקט
```
project_5_noa_binet/
├── CvSiteApi/
│   ├── Controllers/
│   ├── Program.cs
│   ├── appsettings.json
│   └── CvSiteApi.csproj
│
└── CvSiteApi.Services/
    ├── Models/
    ├── GitHubService.cs
    └── CvSiteApi.Services.csproj
```

---

## 🚀 הפעלה
דרישות:
- .NET 8 SDK  
- חשבון GitHub  
- User Secrets להגדרת UserName + Token (פרטי, מקומי בלבד)

הרצה:
```bash
dotnet build
dotnet run
```

---

## 📡 API Endpoints

### 1️⃣ Get Portfolio  
```http
GET /api/github/portfolio
```

**Response Example:**
```json
[
  {
    "id": 123456789,
    "name": "sample-project",
    "language": "C#",
    "stars": 12,
    "pullRequests": 3,
    "lastCommit": "2025-01-14T10:22:45Z",
    "url": "https://github.com/example-user/sample-project",
    "description": "A sample project demonstrating portfolio integration."
  }
]
```

---

### 2️⃣ Search Repositories  
```http
GET /api/github/search?name=clean&language=C%23
```

**Response Example:**
```json
[
  {
    "id": 987654321,
    "name": "clean-architecture-demo",
    "owner": "example-owner",
    "url": "https://github.com/example-owner/clean-architecture-demo",
    "description": "Demo project for Clean Architecture.",
    "language": "C#",
    "stars": 42,
    "forks": 5
  }
]
```

---

## 🧪 בדיקות
Swagger UI:
```
http://localhost:5170/swagger
```

---

## 🎯 תכונות ייחודיות
- Caching חכם  
- Activity Detection לרענון רק בעת פעילות חדשה  
- ביצועים גבוהים והקטנת קריאות ל־GitHub  

---

## 🏗️ ארכיטקטורה
```
Controllers
Services
Models (DTOs)
```

---

## 📝 קבצים חשובים
| קובץ | תיאור |
|------|--------|
| GitHubService.cs | לוגיקת GitHub + Cache |
| GitHubController.cs | נקודות API |
| PortfolioRepositoryDto.cs | מודל פורטפוליו |
| SearchResultDto.cs | מודל חיפוש |

---

## 📚 קישורים שימושיים
- Octokit Docs: https://octokitnet.readthedocs.io/  
- GitHub REST API: https://docs.github.com/en/rest  

---

**Author:** Noa  
**Status:** ✔ Completed

