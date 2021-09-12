# TODO: This should build a .CSPROJ instead of a .SLN
# NOTE: Script assumes that gcloud CLI tool is installed and on PATH
param(
    [string] $projectId = "assessment-grievance-app",
    [string] $serviceName = 'assessment-grievance-service', # <== This is the tag in the GCP Container registry
    [string] $originalDir = (Get-Location),
    [string] $workDir = ((Get-Item $PSScriptRoot).Parent.Parent.FullName),
    [string] $env = "Staging",
    [string] $dotnetEnvVar = "ASPNETCORE_ENVIRONMENT=$env",
    [string] $deployedServiceName = "assessment-grievances-$($env.ToLower())",
    [string[]] $buildSources = @("$workDir\App.sln", "$workDir\Dockerfile", "$workDir\Library", "$workDir\App"),
    [string] $excludeRegex = (@('bin', 'obj', 'e2e', 'node_modules', '__Docs') | ForEach-Object { [Regex]::Escape($_) }) -join '|'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop";
Import-Module -Name "$((Get-Item $PSScriptRoot).Parent.FullName)\_Deploy.Module.psm1" -Force -Verbose

try {
    RunClientAppUnitTests -packageJsonParentDir "$workDir\App\ClientApp"
    RunDotnetUnitTests -unitTestProjectDir "$workDir\App\Tests.Unit"
    
    $dist = "$workDir\Dist"
    if ((Test-Path -Path $dist) -eq $false) {
        New-Item -Path $dist -Force -ItemType 'Directory' -Verbose
    }
    else {
        Get-ChildItem -Path $dist -Recurse | ForEach-Object { Remove-Item $_.FullName -Recurse -Force -Verbose }
    }

    PopulateDistDir `
        -buildSources $buildSources `
        -excludeRegex $excludeRegex `
        -dist $dist `
        -workDir $workDir
    
    $buildArgs = @{ 
        project = $projectId
        buildTag = $serviceName 
        buildSource = $dist
    }
    TriggerBuildOfService @buildArgs    

    $deployArgs = @{ 
        project = $projectId
        buildTag = $serviceName
        cloudRunServiceName = $deployedServiceName 
        dotnetEnvVar = $dotnetEnvVar
    }
    DeployService @deployArgs     
}
catch {
    $_
}
finally {
    Set-Location $originalDir
}

