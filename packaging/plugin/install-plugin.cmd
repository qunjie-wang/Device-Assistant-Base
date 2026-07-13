@echo off
setlocal
chcp 65001 >nul

set "TARGET=%LOCALAPPDATA%\IMRDesktopAssistant"
set "PLUGIN_TARGET=%TARGET%\Plugins\WashroomStatus"

if not exist "%TARGET%\App\IMRDesktopAssistant.exe" (
  echo 未检测到 IMR 设备助手，请先安装基础版。
  pause
  exit /b 1
)

echo 正在安装洗手间状态扩展...
taskkill /F /IM IMRDesktopAssistant.exe >nul 2>&1

if not exist "%PLUGIN_TARGET%" mkdir "%PLUGIN_TARGET%"
xcopy "%~dp0Plugin\*" "%PLUGIN_TARGET%\" /E /I /Y /Q >nul

start "" "%TARGET%\App\IMRDesktopAssistant.exe"

echo.
echo 扩展安装完成。点击托盘图标即可查看状态。
echo.
pause
