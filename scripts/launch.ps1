<#
.SYNOPSIS
    Launches the whole ClineTest application (backend API + frontend) and its infrastructure.

.DESCRIPTION
    Idempotent launcher that drives the stack to a running state, step by step:

      1. Ensures the Docker Compose infrastructure is running (db, localstack, elasticsearch).
      2. Ensures the local database schema is up to date (EF Core migrations).
      3. Ensures the backend (ClientTestApi, launchSettings "https" profile) is running.
      4. Ensures the database is not empty; if it is, runs the seed script.
      5. Ensures the frontend (Vite dev server) is running.

    Already-running pieces are detected and left untouched. The backend and frontend
    are only started when they are not already answering on their ports.

    Each concern streams its logs in its own console window:
      * Docker Compose logs   -> "ClineTest :: Docker Compose logs"
      * EF Core migration     -> "ClineTest :: DB migration"
      * Backend (dotnet run)  -> "ClineTest :: Backend (ClientTestApi HTTPS)"
      * Frontend (vite)       -> "ClineTest :: Frontend (Vite dev)"

.NOTES
    Docker is provided by WSL on this machine, so every docker/compose call is wrapped
    with "wsl.exe --". The seed script talks to the backend over HTTP, therefore the
    backend is started before the (conditional) seed step.
#>
[CmdletBinding()]
param(
    # Skip the "seed the database when it is empty" step.
    [switch]$SkipSeed,

    # Folder handed to the seed script (defaults to the repository "scripts" folder,
    # which contains the movies_2025_json and posters_2025 payload folders).
    [string]$SeedFolder
)

$ErrorActionPreference = 'Stop'

# --- Configuration ----------------------------------------------------------

$scriptRoot = $PSScriptRoot
if (-not $scriptRoot) {
    $scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
}

$RepoRoot    = Split-Path -Parent $scriptRoot
$BackendDir  = Join-Path $RepoRoot 'Backend'
$FrontendDir = Join-Path $RepoRoot 'frontend'
$ScriptsDir  = Join-Path $RepoRoot 'scripts'
$ComposeFile = Join-Path $RepoRoot 'docker-compose.yaml'
$LogDir      = Join-Path $RepoRoot '.logs'

if (-not $SeedFolder) {
    $SeedFolder = $ScriptsDir
}

# Application endpoints (kept in sync with launchSettings.json and vite.config.ts).
$BackendUrl  = 'https://localhost:7072'
$FrontendUrl = 'http://localhost:5173'

# Docker Compose services that make up the application infrastructure.
$InfraServices = @('db', 'localstack', 'elasticsearch')

# Window titles (one console per concern).
$TitleDocker  = 'ClineTest :: Docker Compose logs'
$TitleMigrate = 'ClineTest :: DB migration'
$TitleBackend = 'ClineTest :: Backend (ClientTestApi HTTPS)'
$TitleFrontend = 'ClineTest :: Frontend (Vite dev)'

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null


# --- Helpers ----------------------------------------------------------------

# Windows PowerShell 5.1 (the .NET Framework engine) can fail TLS handshakes against
# the ASP.NET Core dev server ("The underlying connection was closed ... on a send").
# Prefer TLS 1.2 so probes work without tampering with global certificate validation
# (assigning ServerCertificateValidationCallback here actually breaks 5.1 requests).
if ($PSVersionTable.PSVersion.Major -lt 7) {
    try {
        [Net.ServicePointManager]::SecurityProtocol =
            [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls
    }
    catch {
        # Best effort only; some runtimes restrict this assignment.
    }
}

function Write-Step {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host ''
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Write-Ok {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host "    [ok] $Message" -ForegroundColor Green
}

function Write-Info {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host "    [..] $Message" -ForegroundColor Gray
}

function Write-Warn {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host "    [!!] $Message" -ForegroundColor Yellow
}

function ConvertTo-WslPath {
    param([Parameter(Mandatory)][string]$WindowsPath)
    $full = [System.IO.Path]::GetFullPath($WindowsPath)
    $drive = $full.Substring(0, 1).ToLowerInvariant()
    $rest = $full.Substring(2).Replace('\', '/')
    return "/mnt/$drive$rest"
}

function Test-HttpEndpoint {
    param(
        [Parameter(Mandatory)][string]$Url,
        [int]$TimeoutSec = 5
    )
    $iwr = @{
        Uri              = $Url
        UseBasicParsing  = $true
        TimeoutSec       = $TimeoutSec
        ErrorAction      = 'Stop'
    }
    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $iwr['SkipCertificateCheck'] = $true
    }
    try {
        Invoke-WebRequest @iwr | Out-Null
        return $true
    }
    catch [System.Net.WebException] {
        # A response (even an error status) means the server is listening.
        return ($null -ne $_.Exception.Response)
    }
    catch {
        return $false
    }
}

function Wait-HttpEndpoint {
    param(
        [Parameter(Mandatory)][string]$Url,
        [Parameter(Mandatory)][string]$What,
        [int]$TimeoutSec = 240
    )
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    $attempt  = 0
    while ((Get-Date) -lt $deadline) {
        $attempt++
        if (Test-HttpEndpoint -Url $Url) {
            return $true
        }
        if (($attempt % 5) -eq 1) {
            Write-Info "Waiting for $What to answer on $Url ..."
        }
        Start-Sleep -Seconds 3
    }
    return $false
}

function Start-CmdWindow {
    param(
        [Parameter(Mandatory)][string]$Title,
        [Parameter(Mandatory)][string]$Command,
        [string]$WorkingDirectory
    )
    $inner = "title $Title & $Command"
    $sp = @{
        FilePath       = 'cmd.exe'
        ArgumentList   = @('/k', "`"$inner`"")
    }
    if ($WorkingDirectory) {
        $sp['WorkingDirectory'] = $WorkingDirectory
    }
    Start-Process @sp | Out-Null
}

function Get-ContainerHealth {
    param([Parameter(Mandatory)][string]$Name)
    $out = & wsl.exe -- docker inspect -f '{{.State.Health.Status}}' $Name 2>$null
    return ($out | Out-String).Trim()
}

function Wait-ContainersHealthy {
    param(
        [Parameter(Mandatory)][string[]]$Names,
        [int]$TimeoutSec = 240
    )
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        $unhealthy = @()
        foreach ($name in $Names) {
            if ((Get-ContainerHealth -Name $name) -ne 'healthy') {
                $unhealthy += $name
            }
        }
        if ($unhealthy.Count -eq 0) {
            return $true
        }
        Write-Info ("Waiting for healthy containers: {0}" -f ($unhealthy -join ', '))
        Start-Sleep -Seconds 5
    }
    return $false
}

function Get-MovieCount {
    # Returns the number of movies exposed by the API, or -1 when it cannot be determined.
    # The backend is guaranteed to be running at this point, so we ask the API itself for
    # the movie list. This avoids needing a MySQL client on the host and uses the same data
    # source the seed script writes to.
    $iwr = @{
        Uri             = "$BackendUrl/api/movies"
        UseBasicParsing = $true
        TimeoutSec      = 30
        ErrorAction     = 'Stop'
    }
    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $iwr['SkipCertificateCheck'] = $true
    }
    try {
        $response = Invoke-WebRequest @iwr
        $movies = $response.Content | ConvertFrom-Json
        return @($movies).Count
    }
    catch {
        return -1
    }
}


# --- Step 1: Docker Compose infrastructure ---------------------------------

Write-Step 'Ensuring the Docker Compose infrastructure is running'

if (-not (Test-Path $ComposeFile)) {
    throw "docker-compose.yaml not found at '$ComposeFile'."
}

$wslCompose = ConvertTo-WslPath -WindowsPath $ComposeFile

# Explicit "is it already running?" check (same pattern as the backend/frontend steps).
Write-Info 'Checking whether the infrastructure is already running ...'
$runningLines = & wsl.exe -- docker compose -f $wslCompose ps --services --filter status=running 2>&1
$runningSet = @()
foreach ($line in $runningLines) {
    $name = ($line | Out-String).Trim()
    if ($name) {
        $runningSet += $name
    }
}

$missing = @($InfraServices | Where-Object { $runningSet -notcontains $_ })

if ($missing.Count -eq 0) {
    Write-Ok "Infrastructure already running: $($InfraServices -join ', ')"
}
else {
    Write-Info "Starting infrastructure services: $($missing -join ', ')"
    & wsl.exe -- docker compose -f $wslCompose up -d $missing 2>&1 |
        ForEach-Object { "        $_" }
}

Write-Info 'Waiting for db, localstack and elasticsearch to become healthy ...'
if (-not (Wait-ContainersHealthy -Names @('clinetest-mysql', 'clinetest-localstack', 'clinetest-elasticsearch'))) {
    throw 'Infrastructure containers did not become healthy in time. Check the Docker Compose logs window.'
}
Write-Ok 'Infrastructure is up and healthy'

# Stream the infrastructure logs in their own console window.
Start-CmdWindow -Title $TitleDocker -WorkingDirectory $RepoRoot -Command "wsl.exe -- docker compose -f $wslCompose logs -f --tail=200 $($InfraServices -join ' ')"
Write-Ok "Docker Compose logs are streaming in a separate window ($TitleDocker)"

# --- Step 2: Database schema (EF Core migrations) ---------------------------

Write-Step 'Ensuring the local database schema is up to date (EF Core migrations)'

$migExitFile = Join-Path $LogDir 'migration.exit'
$migRunner   = Join-Path $LogDir 'run-migration.cmd'
Remove-Item -Force $migExitFile -ErrorAction SilentlyContinue

$migRunnerBody = @"
@echo off
setlocal
cd /d "$BackendDir"
echo ============================================================
echo  ClineTest - EF Core database migration
echo ============================================================
echo.
echo [1/2] Restoring local .NET tools (dotnet-ef, ...)
dotnet tool restore
if errorlevel 1 (
    echo dotnet tool restore failed.
    goto :finish
)
echo.
echo [2/2] Applying EF Core migrations to the database
dotnet ef database update --project ClineTestInfra --startup-project ClientTestApi
:finish
echo.
echo Migration finished with exit code %errorlevel%
echo MIGRATION_EXIT_CODE=%errorlevel% > "$migExitFile"
echo ============================================================
echo  Migration window finished. You can close this window.
echo ============================================================
"@
Set-Content -Path $migRunner -Value $migRunnerBody -Encoding ASCII

Start-Process -FilePath 'cmd.exe' -ArgumentList @('/k', "`"$migRunner`"") | Out-Null
Write-Info "EF Core migration is running in a separate window ($TitleMigrate) ..."

$migDeadline = (Get-Date).AddSeconds(600)
while (-not (Test-Path $migExitFile) -and (Get-Date) -lt $migDeadline) {
    Start-Sleep -Seconds 2
}

if (-not (Test-Path $migExitFile)) {
    throw 'The migration did not finish in time. Check the migration window for details.'
}

$migCode = -1
$migMatch = [regex]::Match((Get-Content -Raw $migExitFile), 'MIGRATION_EXIT_CODE=(\d+)')
if ($migMatch.Success) {
    $migCode = [int]$migMatch.Groups[1].Value
}
if ($migCode -ne 0) {
    throw "Database migration failed with exit code $migCode. See the '$TitleMigrate' window and $LogDir."
}
Write-Ok 'Database schema is up to date'


# --- Step 3: Backend (ClientTestApi, "https" profile) -----------------------

Write-Step 'Ensuring the backend (ClientTestApi) is running'

if (Test-HttpEndpoint -Url "$BackendUrl/api/movies") {
    Write-Ok "Backend is already running and answering on $BackendUrl"
}
else {
    Write-Info "Starting the backend in a separate window ($TitleBackend) ..."
    Start-CmdWindow -Title $TitleBackend -WorkingDirectory $BackendDir -Command 'dotnet run --project ClientTestApi --launch-profile https'

    if (-not (Wait-HttpEndpoint -Url "$BackendUrl/api/movies" -What 'the backend API' -TimeoutSec 300)) {
        throw "The backend did not become ready on $BackendUrl in time. Check the '$TitleBackend' window."
    }
    Write-Ok "Backend is running on $BackendUrl"
}

# --- Step 4: Seed the database when it is empty -----------------------------

if ($SkipSeed) {
    Write-Step 'Skipping the seed step (-SkipSeed was provided)'
}
else {
    Write-Step 'Ensuring the database is not empty'

    $movieCount = Get-MovieCount
    if ($movieCount -lt 0) {
        Write-Warn 'Could not determine the movie count; skipping the seed step.'
    }
    elseif ($movieCount -eq 0) {
        Write-Info 'The database is empty; running the seed script ...'
        $seedScript = Join-Path $ScriptsDir 'seed-database.py'
        if (-not (Test-Path $seedScript)) {
            throw "Seed script not found at '$seedScript'."
        }
        & python $seedScript $SeedFolder 2>&1 | ForEach-Object { "        $_" }
        if ($LASTEXITCODE -ne 0) {
            Write-Warn "The seed script exited with code $LASTEXITCODE. Check its output above."
        }
        else {
            Write-Ok "Database seeded from '$SeedFolder'"
        }
    }
    else {
        Write-Ok "Database already contains $movieCount movie(s); skipping the seed step"
    }
}

# --- Step 5: Frontend (Vite dev server) -------------------------------------

Write-Step 'Ensuring the frontend (Vite dev server) is running'

if (-not (Test-Path (Join-Path $FrontendDir 'node_modules'))) {
    Write-Info 'node_modules is missing; installing dependencies (npm install) ...'
    Push-Location $FrontendDir
    try {
        & npm.cmd install 2>&1 | ForEach-Object { "        $_" }
    }
    finally {
        Pop-Location
    }
}

if (Test-HttpEndpoint -Url $FrontendUrl) {
    Write-Ok "Frontend is already running and answering on $FrontendUrl"
}
else {
    Write-Info "Starting the frontend in a separate window ($TitleFrontend) ..."
    Start-CmdWindow -Title $TitleFrontend -WorkingDirectory $FrontendDir -Command 'npm run dev'

    if (-not (Wait-HttpEndpoint -Url $FrontendUrl -What 'the frontend' -TimeoutSec 180)) {
        throw "The frontend did not become ready on $FrontendUrl in time. Check the '$TitleFrontend' window."
    }
    Write-Ok "Frontend is running on $FrontendUrl"
}

# --- Summary ----------------------------------------------------------------

Write-Step 'ClineTest is up'
Write-Host "    Frontend : $FrontendUrl" -ForegroundColor White
Write-Host "    Backend  : $BackendUrl  (Swagger: $BackendUrl/swagger)" -ForegroundColor White
Write-Host "    Movies   : $BackendUrl/api/movies" -ForegroundColor White
Write-Host ''
Write-Host '    Logs are streaming in the separate console windows opened above.' -ForegroundColor Gray
Write-Host ''

