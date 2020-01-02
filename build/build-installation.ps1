param(
#      [string]$cmd,
      [string]$HQSourcePath, 
	  [switch]$noDestCleanup)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Web.Extensions
function setupExportService($exportSettingsPath) {
    $parser = New-Object Web.Script.Serialization.JavaScriptSerializer
    $exportSettings = $parser.DeserializeObject((Get-Content $exportSettingsPath -raw))
    
    $exportSettings.ConnectionStrings.DefaultConnection = "Provided by HQ"
    $exportSettings.Storage.S3.Enabled = $false

    if($exportSettings.ExportSettings -eq $null) {
        $exportSettings.ExportSettings = @{

        }
    }

    $exportSettings.ExportSettings.DirectoryPath = "..\..\..\Data_Site\ExportServiceData"

    $exportSettings | ConvertTo-Json -Depth 100 | set-content $exportSettingsPath
}

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\functions.ps1"

$InstallationProject = 'src\Installation\SurveySolutions\SurveySolutionsBootstrap\SurveySolutionsBootstrap.wixproj'

$sourceCleanup = $False

$workdir = Get-Location
if ($HQSourcePath -eq "") {
	$HQSourcePath = Join-Path $workdir "HQpackage"
	$sourceCleanup = $True
}

#Set-Location $HQSourcePath
$sitePatha = (Get-ChildItem $HQSourcePath -recurse | Where-Object {$_.PSIsContainer -eq $true -and $_.Name -match "PackageTmp"}).FullName

$HQsitePath = Join-path $workdir "HQwork"
if (!(Test-Path $HQsitePath)) {
	New-Item $HQsitePath -ItemType Directory
	New-Item (Join-Path $HQsitePath "App_Data") -ItemType Directory
}

$supportPath = Join-path $workdir "SupportPackage"
$targetSupportPath = Join-path $HQsitePath "Support"

Copy-Item $sitePatha\* $HQsitePath -Force -Recurse
Remove-Item "$HQsitePath\HostMap.config"

Copy-Item $HQSourcePath\ExportService $HQsitePath\.bin\Export -Force -Recurse
Copy-Item -Path $supportPath -Destination $targetSupportPath -Force -Recurse

$file = (Get-ChildItem -Path $HQsitePath -recurse | Where-Object {$_.Name -match "WB.UI.Headquarters.dll"})
$version = [Reflection.AssemblyName]::GetAssemblyName($file.FullName).Version

setupExportService "$HQsitePath\.bin\Export\appsettings.json"

# Cleaning up slack configuration section from config
$hqConfig = "$HQsitePath\Web.config"
[xml]$xml = Get-Content $hqConfig
$node = $xml.SelectSingleNode("//slack")
$node.ParentNode.RemoveChild($node)
$xml.save($hqConfig)

$installationArgs = @(
    $InstallationProject;
    '/t:Build';
    "/p:HarvestDir=$HQsitePath";
    "/p:HarvestDirectory=$HQsitePath";
    "/p:Configuration=Release";
    "/p:Platform=x64";
    "/p:SurveySolutionsVersion=$version";
)

& (GetPathToMSBuild) $installationArgs | Write-Host

$wasBuildSuccessfull = $LASTEXITCODE -eq 0

if (-not $wasBuildSuccessfull) {
    Write-Host "##teamcity[message status='ERROR' text='Failed to build installation']"
    if (-not $MultipleSolutions) {
        Write-Host "##teamcity[buildProblem description='Failed to build installation']"
    }
}

Set-Location $workdir

if (!($noDestCleanup)) {
	Remove-Item $HQsitePath -Force -Recurse
}
if ($sourceCleanup) {
	Remove-Item $HQSourcePath -Force -Recurse
}

Write-Host "##teamcity[publishArtifacts 'SurveySolutions.msi']"
