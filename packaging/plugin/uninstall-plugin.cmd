@echo off
setlocal
chcp 65001 >nul

set "TARGET=%LOCALAPPDATA%\IMRDesktopAssistant"
set "PLUGIN_TARGET=%TARGET%\Plugins\WashroomStatus"

echo 正在卸载洗手间状态扩展...
taskkill /F /IM IMRDesktopAssistant.exe >nul 2>&1
rmdir /S /Q "%PLUGIN_TARGET%" >nul 2>&1

if exist "%TARGET%\App\IMRDesktopAssistant.exe" (
  start "" "%TARGET%\App\IMRDesktopAssistant.exe"
)

echo 扩展已卸载，设备信息功能不受影响。
pause
