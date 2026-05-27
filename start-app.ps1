param(
    [switch]$Backend,
    [switch]$Frontend,
    [switch]$Expose
)

$root = $PSScriptRoot

# Determine which services to start
$runBackend  = $Backend  -or (-not $Frontend)
$runFrontend = $Frontend -or (-not $Backend)

# Determine host binding
if ($Expose) {
    $backendHost  = '0.0.0.0'
    $frontendHost = '0.0.0.0'
    $localIp = (Get-NetIPAddress -AddressFamily IPv4 |
        Where-Object { $_.PrefixOrigin -ne 'WellKnown' -and $_.IPAddress -ne '127.0.0.1' } |
        Select-Object -First 1).IPAddress
    if (-not $localIp) { $localIp = 'localhost' }
} else {
    $backendHost  = 'localhost'
    $frontendHost = 'localhost'
    $localIp      = 'localhost'
}

$backendUrls = "http://${backendHost}:5243"
$total = ([int]$runBackend + [int]$runFrontend)
$step  = 1

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "    Starting Family Trip Advisor App      " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

if ($runBackend) {
    Write-Host "[$step/$total] Opening backend window..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-NoExit", "-Command", `
        "Set-Location '$root'; Write-Host 'Backend starting...' -ForegroundColor Green; dotnet run --project backend --urls '$backendUrls'"
    $step++
}

if ($runFrontend) {
    Write-Host "[$step/$total] Opening frontend window..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-NoExit", "-Command", `
        "Set-Location '$root\frontend'; Write-Host 'Frontend starting...' -ForegroundColor Green; npm start -- --host '$frontendHost'"
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
if ($runBackend -and $runFrontend) {
    Write-Host " Both services are starting up!"    -ForegroundColor Cyan
} elseif ($runBackend) {
    Write-Host " Backend is starting up!"           -ForegroundColor Cyan
} else {
    Write-Host " Frontend is starting up!"          -ForegroundColor Cyan
}
Write-Host " (check the new window(s) for progress)" -ForegroundColor Cyan
Write-Host ""

if ($runFrontend) {
    Write-Host " Open your browser at:" -ForegroundColor White
    Write-Host "   http://${localIp}:4200" -ForegroundColor Yellow
    if ($Expose -and $localIp -ne 'localhost') {
        Write-Host "   (accessible on your local network)" -ForegroundColor Yellow
    }
    Write-Host ""
}

Write-Host " To stop: close the opened window(s)." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
