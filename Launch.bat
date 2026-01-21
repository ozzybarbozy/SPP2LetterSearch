@echo off
REM Kill all existing instances
taskkill /IM SPP2LetterSearch.exe /F >nul 2>&1

REM Wait a moment
timeout /t 1 /nobreak >nul

REM Launch fresh instance
start "" "c:\LetterMaster\SPP2LetterSearch\bin\Debug\net8.0-windows\SPP2LetterSearch.exe"

exit
