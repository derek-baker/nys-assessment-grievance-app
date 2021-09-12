# NOTE: Functions assume that gcloud CLI tool and NodeJS/NPM is installed and on PATH

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop";
#Requires -Version 5.1

function runCmdAndCaptureOutput(
    [Parameter(Mandatory=$true)]
    [string] $cmd
) {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUserDeclaredVarsMoreThanAssignments', '')]
    [string] $errOut = ""

    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUserDeclaredVarsMoreThanAssignments', '' )]
    [string] $stdOut = ""

    Invoke-Expression $cmd -ErrorVariable errOut -OutVariable stdOut   
    if($LASTEXITCODE -ne 0) {
        Write-Host -ForegroundColor Red "LASTEXITCODE: $LASTEXITCODE"
        throw $LASTEXITCODE
    }
}

function RunClientAppUnitTests(
    [Parameter(Mandatory=$true)]
    [string] $packageJsonParentDir
) {
    $initialLocation = Get-Location

    try {
        Set-Location $packageJsonParentDir
        $cmd = 'npm run unit-test'
        runCmdAndCaptureOutput -cmd $cmd
	}
    catch {
        $_
        exit 1
	}
    finally {
        Set-Location $initialLocation
	}    
}

function RunDotnetUnitTests(
    [Parameter(Mandatory=$true)]
    [string] $unitTestProjectDir
) {
    $initialLocation = Get-Location

    try {
        Set-Location $unitTestProjectDir
        $cmd = 'dotnet test'
        runCmdAndCaptureOutput -cmd $cmd
	}
    catch {
        $_
        exit 1
	}
    finally {
        Set-Location $initialLocation
	}
}

function PopulateDistDir(
    [Parameter(mandatory=$true)]
    [string[]] $buildSources,

    [Parameter(mandatory=$true)]
    [string] $excludeRegex,

    [Parameter(mandatory=$true)]
    [string] $dist,

    [Parameter(mandatory=$true)]
    [string] $workDir
) {
    $buildSources | ForEach-Object {
        $itemToCopy = Get-Item -Path $_ 
        
        # We need to exclude some dirs from being copied into dist
        if (($itemToCopy.PSIsContainer -eq $true)) {
            Get-ChildItem -Path $itemToCopy -File -Recurse | `
                Where-Object { $_.DirectoryName -notmatch $excludeRegex } | ` # <== WARNING: This does a wildcard match (ex: If dir name contains 'obj' it will be ignored)
                    ForEach-Object {
                        $target = Join-Path -Path $dist -ChildPath $_.DirectoryName.Substring($workDir.Length)
                        if (!(Test-Path -Path $target -PathType Container)) { 
                            New-Item -Path $target -ItemType Directory -Verbose | Out-Null 
                        }
                        $_ | Copy-Item -Destination $target -Force -Verbose
                    }
        }       
        else {
            Copy-Item -Path $itemToCopy -Recurse -Destination $dist -Verbose
        }         
    }
}

function TriggerBuildOfService(
    [Parameter(mandatory=$true)]
    [string] $project, 

    [Parameter(mandatory=$true)]
    [string] $buildTag,

    [Parameter(mandatory=$true)]
    [string] $buildSource,

    [Parameter(mandatory=$false)]
    [string] $timeoutSeconds = '900'
) {
    [string] $cmd = "gcloud builds submit --tag 'gcr.io/$project/$buildTag' $buildSource --timeout=$timeoutSeconds" 
    runCmdAndCaptureOutput -cmd $cmd
}

function DeployService(
    [Parameter(mandatory=$true)]
    [string] $project, 

    [Parameter(mandatory=$true)]
    [string] $buildTag,

    [Parameter(mandatory=$true)]
    [string] $cloudRunServiceName,

    [Parameter(mandatory=$true)]
    [string] $dotnetEnvVar,

    [Parameter(mandatory=$false)]
    [bool] $allowUnauthenticated = $true,

    [Parameter(mandatory=$false)]
    [int] $maxInstances = 10,

    [Parameter(mandatory=$false)]
    [string] $cpu = '1',

    [Parameter(mandatory=$false)]
    [string] $memory = '512Mi' 
) {
    $allowPublicAccess = if ($allowUnauthenticated -eq $true) { '--allow-unauthenticated' } else { '--no-allow-unauthenticated' }
    gcloud run deploy $cloudRunServiceName --image "gcr.io/$project/$buildTag" --platform managed --max-instances=$maxInstances --set-env-vars=$dotnetEnvVar --cpu=$cpu --memory=$memory $allowPublicAccess
}

Export-ModuleMember -Function TriggerBuildOfService, DeployService, RunClientAppUnitTests, RunDotnetUnitTests, PopulateDistDir


