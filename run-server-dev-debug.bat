set ASPNETCORE_ENVIRONMENT=Development

start "MiniServerProject" dotnet run --project MiniServerProject\MiniServerProject.csproj --launch-profile "https"

timeout /t 5 >nul
start "" https://localhost:7165/swagger

pause