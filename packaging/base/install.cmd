@echo off
setlocal
chcp 65001 >nul

set "TARGET=%LOCALAPPDATA%\IMRDesktopAssistant"
set "APP_TARGET=%TARGET%\App"

echo 正在安装 IMR 设备助手...
taskkill /F /IM IMRDesktopAssistant.exe >nul 2>&1

if not exist "%APP_TARGET%" mkdir "%APP_TARGET%"
xcopy "%~dp0App\*" "%APP_TARGET%\" /E /I /Y /Q >nul

start "" "%APP_TARGET%\IMRDesktopAssistant.exe"

echo.
echo 安装完成。程序已启动，并会为当前用户开启登录自启动。
echo.
pause
