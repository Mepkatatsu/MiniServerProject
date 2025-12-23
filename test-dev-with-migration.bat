@echo off
set ASPNETCORE_ENVIRONMENT=Development

dotnet ef database update --project MiniServerProject\MiniServerProject.csproj
dotnet test

pause