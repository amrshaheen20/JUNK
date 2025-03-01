@echo off
setlocal enabledelayedexpansion

set "FRONTEND_DIR=ChatApp.client"
set "BACKEND_DIR=ChatApp.server"
set "MIGRATIONS_DIR=%BACKEND_DIR%\Migrations"

goto :main

:error_handler
echo.
echo Error occurred in step: !current_step!
echo Press any key to exit...
pause >nul
exit /b 1

:main
echo ===========================
echo Starting build process...
echo ===========================
echo.

set "current_step=Frontend setup"
echo [Frontend] Installing dependencies...
cd /d "%~dp0%FRONTEND_DIR%" || goto error_handler
start "npm install" /wait cmd /c "npm install && exit 0 || exit 1"
if %errorlevel% neq 0 goto error_handle

echo [Frontend] npm install completed successfully!

set "current_step=Backend setup"
echo [Backend] Building solution...
cd /d "%~dp0%BACKEND_DIR%" || goto error_handler
dotnet build || goto error_handler

if not exist "%MIGRATIONS_DIR%" (
    echo [Backend] Migrations folder not found, creating initial migration...
    dotnet ef migrations add Initial || goto error_handler
    dotnet ef database update || goto error_handler
)


cd /d "%~dp0%"  

echo [Frontend] Starting development server...
start "Frontend Server" cmd /k "cd /d "%~dp0%FRONTEND_DIR%" && npm run dev"

echo [Backend] Starting backend server...
start "Backend Server" cmd /k "cd /d "%~dp0%BACKEND_DIR%" && dotnet run"

echo.
echo Both services should now be running in separate windows
echo Press any key to exit this script...
pause >nul
exit /b 0
