$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$out = Join-Path $root 'artifacts'
$baseStage = Join-Path $out 'IMR-Device-Assistant-Base'
$pluginStage = Join-Path $out 'IMR-Washroom-Plugin'

Remove-Item $out -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path (Join-Path $baseStage 'App') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $pluginStage 'Plugin') -Force | Out-Null

dotnet publish (Join-Path $root 'src\IMRDesktopAssistant\IMRDesktopAssistant.csproj') `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=false -p:PublishReadyToRun=true `
  -o (Join-Path $baseStage 'App')

dotnet build (Join-Path $root 'src\IMRDesktopAssistant.Plugin.WashroomStatus\IMRDesktopAssistant.Plugin.WashroomStatus.csproj') `
  -c Release

Copy-Item (Join-Path $root 'packaging\base\*.cmd') $baseStage
Copy-Item (Join-Path $root 'src\IMRDesktopAssistant.Plugin.WashroomStatus\bin\Release\net8.0-windows\IMRDesktopAssistant.Plugin.WashroomStatus.dll') (Join-Path $pluginStage 'Plugin')
Copy-Item (Join-Path $root 'packaging\plugin\washroom.json') (Join-Path $pluginStage 'Plugin')
Copy-Item (Join-Path $root 'packaging\plugin\*.cmd') $pluginStage

Compress-Archive -Path "$baseStage\*" -DestinationPath (Join-Path $out 'IMR-Device-Assistant-Base.zip') -Force
Compress-Archive -Path "$pluginStage\*" -DestinationPath (Join-Path $out 'IMR-Washroom-Plugin.zip') -Force

Write-Host "Build complete: $out"
