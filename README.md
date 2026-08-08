# GitHub Portfolio API

An ASP.NET Core 8 API that turns GitHub repository data into a portfolio-ready feed. It returns enriched repository cards, supports public repository search, and uses an activity-aware in-memory cache to reduce unnecessary API calls.

Originally developed in December 2025. Tests, CI, configuration validation, and reliability improvements were added during portfolio hardening in August 2026.

## What it demonstrates

- ASP.NET Core controllers and dependency injection
- GitHub integration through Octokit
- DTO-based API contracts
- Five-minute caching with GitHub activity invalidation
- Graceful stale-cache fallback when GitHub is unavailable
- Safe upstream error handling through generic `502` responses
- Automated controller tests and GitHub Actions

## Architecture

```text
HTTP request
    |
GitHubController
    |
IGitHubService
    |
GitHubService ---- in-memory cache
    |
Octokit / GitHub API
```

The repository contains two production projects:

- `CvSiteApi` — HTTP endpoints, configuration, and dependency injection.
- `CvSiteApi.Services` — GitHub access, caching, activity detection, and DTO mapping.

## API

### Portfolio

```http
GET /api/github/portfolio
```

Returns the configured user's public repositories with language, stars, pull-request count, latest commit date, URL, and description.

### Repository search

```http
GET /api/github/search?name=clean&language=C%23&user=dotnet
```

At least one of `name`, `language`, or `user` is required.

## Run locally

Requirements: .NET 8 SDK and a GitHub username. A token is optional for public data but recommended for higher API rate limits.

```powershell
dotnet user-secrets set "GitHub:UserName" "YOUR_GITHUB_USERNAME" --project CvSiteApi
dotnet user-secrets set "GitHub:Token" "YOUR_GITHUB_TOKEN" --project CvSiteApi
dotnet run --project CvSiteApi
```

Swagger is available in Development at the URL printed by ASP.NET Core, followed by `/swagger`.

Environment variables can be used instead of User Secrets:

```text
GitHub__UserName=YOUR_GITHUB_USERNAME
GitHub__Token=YOUR_GITHUB_TOKEN
```

Never commit a real token. `appsettings.json` intentionally contains empty placeholders.

## Quality checks

```powershell
dotnet restore tests/CvSiteApi.Tests/CvSiteApi.Tests.csproj
dotnet build tests/CvSiteApi.Tests/CvSiteApi.Tests.csproj --configuration Release --no-restore
dotnet test tests/CvSiteApi.Tests/CvSiteApi.Tests.csproj --configuration Release --no-build
dotnet publish CvSiteApi/CvSiteApi.csproj --configuration Release --no-restore
```

GitHub Actions runs the same build, test, and publish checks on every push and pull request.
