# Flock

Church communication that everyone actually uses.

This repository contains the first Flock MVP vertical slice: a mobile-first member
experience backed by an ASP.NET Core API. It supports groups, announcements,
events, RSVPs, volunteer positions, signups, notifications, and a small admin
overview.

## Run locally

Prerequisite: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
dotnet run --project services/api/Flock.Api.csproj
```

Open the URL printed by ASP.NET (normally `http://localhost:5000`). The API
serves the responsive client from `apps/web-admin`.

Demo identity: `jordan@gracecommunity.org` / any password of 6+ characters.
Authentication is intentionally marked as development-only in the interface;
production OAuth, password hashing, persistence, and delivery providers are
tracked in the roadmap.

## Repository

- `apps/web-admin` — responsive member and administrator client
- `apps/mobile` — React Native boundary and implementation notes
- `services/api` — ASP.NET Core REST API and static hosting
- `services/notification-service` — provider boundary documentation
- `database` — schema and seed design
- `docs` — architecture, API, security, and roadmap decisions
- `infrastructure` — local and Azure deployment placeholders
- `scripts` — developer workflows

## MVP boundary

The implementation deliberately uses an in-memory repository so it has no
third-party runtime packages. It is a working product slice, not a production
release. Before beta, replace the repository with PostgreSQL, add JWT/OAuth,
connect push/email delivery, and add integration tests against real infrastructure.

