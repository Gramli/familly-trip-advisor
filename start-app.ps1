$root = $PSScriptRoot

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "    Starting Family Trip Advisor App      " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Start the .NET backend in its own window
Write-Host "[1/2] Opening backend window..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", `
    "Set-Location '$root'; Write-Host 'Backend starting...' -ForegroundColor Green; dotnet run --project backend --urls 'http://0.0.0.0:5243'"

# Start the Angular frontend in its own window
Write-Host "[2/2] Opening frontend window..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", `
    "Set-Location '$root\frontend'; Write-Host 'Frontend starting...' -ForegroundColor Green; npm start -- --host 0.0.0.0"

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " Both services are starting up!"           -ForegroundColor Cyan
Write-Host " (check the two new windows for progress)" -ForegroundColor Cyan
Write-Host ""
Write-Host " Open your browser at:" -ForegroundColor White
Write-Host "   http://localhost:4200" -ForegroundColor Yellow
Write-Host ""
Write-Host " To stop: close the backend and frontend windows." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
