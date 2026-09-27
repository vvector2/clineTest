<#
.SYNOPSIS
    Runs a SonarQube analysis of the Backend .NET solution.

.DESCRIPTION
    Builds Backend/Backend.sln with the SonarScanner for .NET and uploads the
    analysis to the SonarQube server started by 'docker compose up -d sonarqube'.

    The authentication token is read from the SONAR_TOKEN_CLINETEST environment
    variable. When the variable is not set or empty an information message is
    written and no analysis is started.

.PARAMETER ProjectKey
    Key of the SonarQube project the analysis is attached to. Defaults to 'ClineTest'.

.PARAMETER HostUrl
    Base URL of the SonarQube server. Defaults to 'http://localhost:9000'.

.EXAMPLE
    $env:SONAR_TOKEN_CLINETEST = '<your-token>'
    ./scripts/sonar.ps1

.EXAMPLE
    ./scripts/sonar.ps1 -HostUrl 'https://sonar.example.com'
#>
[CmdletBinding()]
param
(
    [string] $ProjectKey = 'ClineTest',

    [string] $HostUrl = 'http://localhost:9000'
)

$ErrorActionPreference = 'Stop'

$tokenVariableName = 'SONAR_TOKEN_CLINETEST'
$repoRoot = Split-Path -Parent $PSScriptRoot
$backendDirectory = Join-Path -Path $repoRoot -ChildPath 'Backend'
$solutionPath = Join-Path -Path $backendDirectory -ChildPath 'Backend.sln'

function Write-InfoMessage
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Message
    )

    Write-Host "INFO: $Message" -ForegroundColor Cyan
}

function Invoke-DotnetCommand
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Description,

        [Parameter(Mandatory = $true)]
        [string[]] $Arguments
    )

    Write-Host "==> $Description" -ForegroundColor DarkGray

    & dotnet @Arguments | Out-Host

    return ($LASTEXITCODE -eq 0)
}

function Test-SonarServer
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Url
    )

    try
    {
        $status = Invoke-RestMethod -Method Get -Uri "$Url/api/system/status" -TimeoutSec 10
    }
    catch
    {
        throw "SonarQube is not reachable at '$Url'. Start it with 'docker compose up -d sonarqube'. ($($_.Exception.Message))"
    }

    if ($status.status -ne 'UP')
    {
        throw "SonarQube at '$Url' reports status '$($status.status)'."
    }

    Write-InfoMessage "SonarQube at '$Url' is up (version $($status.version))."
}

$token = [Environment]::GetEnvironmentVariable($tokenVariableName)

if ([string]::IsNullOrWhiteSpace($token))
{
    Write-InfoMessage "Environment variable $tokenVariableName is not set or empty."
    Write-InfoMessage "The SonarQube analysis is skipped. Set the token and run the script again, for example: `$env:$tokenVariableName = '<your-token>'"
    return
}

if (-not (Test-Path -LiteralPath $solutionPath))
{
    throw "The solution '$solutionPath' was not found."
}

Test-SonarServer -Url $HostUrl

Push-Location -Path $backendDirectory

try
{
    if (-not (Invoke-DotnetCommand -Description 'Restoring the local dotnet tools' -Arguments @('tool', 'restore')))
    {
        throw 'The local dotnet tools could not be restored.'
    }

    $beginArguments = @('sonarscanner', 'begin', "/k:$ProjectKey", "/d:sonar.host.url=$HostUrl", "/d:sonar.token=$token")

    if (-not (Invoke-DotnetCommand -Description "Starting the SonarQube analysis of '$ProjectKey' on $HostUrl" -Arguments $beginArguments))
    {
        throw 'The SonarQube analysis could not be started. Check that the token is valid and has analysis permissions.'
    }

    $succeeded = $false

    try
    {
        $succeeded = Invoke-DotnetCommand -Description "Building '$solutionPath'" -Arguments @('build', $solutionPath, '--no-incremental', '--disable-build-servers')
    }
    finally
    {
        if (-not (Invoke-DotnetCommand -Description 'Uploading the analysis to SonarQube' -Arguments @('sonarscanner', 'end', "/d:sonar.token=$token")))
        {
            $succeeded = $false
        }
    }

    if (-not $succeeded)
    {
        throw 'The SonarQube analysis failed. See the output above for details.'
    }
}
finally
{
    Pop-Location
}

Write-InfoMessage "SonarQube analysis of '$ProjectKey' completed. Results: $HostUrl/dashboard?id=$ProjectKey"

