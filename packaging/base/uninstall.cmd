@echo off
setlocal
chcp 65001 >nul

set "TARGET=%LOCALAPPDATA%\IMRDesktopAssistant"

echo 正在卸载 IMR 设备助手...
taskkill /F /IM IMRDesktopAssistant.exe >nul 2>&1
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "IMRDesktopAssistant" /f >nul 2>&1
rmdir /S /Q "%TARGET%" >nul 2>&1

echo 卸载完成。
pause
