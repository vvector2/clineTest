<#
.SYNOPSIS
    Exports the High, Critical and Medium SonarQube issues of a project to a JSON file.

.DESCRIPTION
    Reads the unresolved issues of the project from the SonarQube server started by
    'docker compose up -d sonarqube' and collects the issues whose impact severity is
    BLOCKER, HIGH or MEDIUM together with the issues whose legacy severity is BLOCKER,
    CRITICAL or MAJOR in a single JSON report.

    The report is written to scripts/sonar-report-issues.json. Every issue is listed
    once, even when it matches both severity filters.

    The authentication token is read from the SONAR_TOKEN_CLINETEST environment
    variable. When the variable is not set or empty an information message is
    written and no report is created.

.PARAMETER ProjectKey
    Key of the SonarQube project the issues are read from. Defaults to 'ClineTest'.

.PARAMETER HostUrl
    Base URL of the SonarQube server. Defaults to 'http://localhost:9000'.

.EXAMPLE
    $env:SONAR_TOKEN_CLINETEST = '<your-token>'
    ./scripts/sonar-issues.ps1

.EXAMPLE
    ./scripts/sonar-issues.ps1 -ProjectKey 'OtherProject' -HostUrl 'https://sonar.example.com'
#>
[CmdletBinding()]
param
(
    [string] $ProjectKey = 'ClineTest',

    [string] $HostUrl = 'http://localhost:9000'
)

$ErrorActionPreference = 'Stop'

$tokenVariableName = 'SONAR_TOKEN_CLINETEST'
$reportPath = Join-Path -Path $PSScriptRoot -ChildPath 'sonar-report-issues.json'
$pageSize = 100
$severityFilters = @('impactSeverities=BLOCKER,HIGH,MEDIUM', 'severities=BLOCKER,CRITICAL,MAJOR')

function Write-InfoMessage
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Message
    )

    Write-Host "INFO: $Message" -ForegroundColor Cyan
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

function Get-SonarIssues
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Url,

        [Parameter(Mandatory = $true)]
        [string] $Project,

        [Parameter(Mandatory = $true)]
        [string] $Filter,

        [Parameter(Mandatory = $true)]
        [hashtable] $Headers
    )

    $issues = New-Object System.Collections.Generic.List[object]
    $page = 1

    while ($true)
    {
        $uri = "$Url/api/issues/search?componentKeys=$Project&resolved=false&ps=$pageSize&p=$page&$Filter"
        $response = Invoke-RestMethod -Method Get -Uri $uri -Headers $Headers -TimeoutSec 60

        foreach ($issue in $response.issues)
        {
            $issues.Add($issue)
        }

        if (($page * $pageSize) -ge $response.total)
        {
            break
        }

        $page++
    }

    return $issues
}

$token = [Environment]::GetEnvironmentVariable($tokenVariableName)

if ([string]::IsNullOrWhiteSpace($token))
{
    Write-InfoMessage "Environment variable $tokenVariableName is not set or empty."
    Write-InfoMessage "The SonarQube issues are not exported. Set the token and run the script again, for example: `$env:$tokenVariableName = '<your-token>'"
    return
}

Test-SonarServer -Url $HostUrl

$headers = @{ Authorization = "Bearer $token" }
$issuesByKey = [ordered]@{}

foreach ($severityFilter in $severityFilters)
{
    Write-InfoMessage "Reading the unresolved issues of '$ProjectKey' matching '$severityFilter'."

    foreach ($issue in (Get-SonarIssues -Url $HostUrl -Project $ProjectKey -Filter $severityFilter -Headers $headers))
    {
        $issuesByKey[$issue.key] = $issue
    }
}

$issues = @($issuesByKey.Values)
$json = ConvertTo-Json -InputObject $issues -Depth 12

[System.IO.File]::WriteAllText($reportPath, $json)

Write-InfoMessage "$($issues.Count) High, Critical or Medium issues of '$ProjectKey' written to '$reportPath'."
