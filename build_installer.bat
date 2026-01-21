@echo off
echo Building SPP2 Letter Search Installer...
echo.

set ISCC="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not exist %ISCC% set ISCC="C:\Program Files\Inno Setup 6\ISCC.exe"
if not exist %ISCC% (
    echo ERROR: Inno Setup not found!
    echo Please install Inno Setup 6 from https://jrsoftware.org/isinfo.php
    echo Or run: winget install JRSoftware.InnoSetup
    pause
    exit /b 1
)

%ISCC% "%~dp0setup.iss"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS! Installer created in: %~dp0installer\
    echo File: SPP2LetterSearch_Setup.exe
) else (
    echo.
    echo ERROR: Failed to build installer.
)

pause
