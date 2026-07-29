$ErrorActionPreference = "Stop"
dotnet format services/api/Flock.Api.csproj --verify-no-changes
dotnet build services/api/Flock.Api.csproj
dotnet run --project services/api.tests/Flock.Api.Tests.csproj
node --check apps/web-admin/app.js

