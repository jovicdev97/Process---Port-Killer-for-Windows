@echo off
echo Building PortKiller application...

REM Clean previous builds
if exist "dist" (
    echo Cleaning previous build...
    rmdir /s /q "dist"
)

REM Check if dotnet is available
where dotnet >nul 2>&1
if %ERRORLEVEL% == 0 (
    echo Using dotnet CLI to build...
    
    REM Create self-contained executable in dist folder
    echo Creating standalone executable...
    dotnet publish PortKiller.GUI --configuration Release --self-contained true --runtime win-x64 --output dist
    
    if %ERRORLEVEL% == 0 (
        echo.
        echo ============================================
        echo BUILD SUCCESSFUL!
        echo ============================================
        echo Standalone executable: dist\PortKiller.GUI.exe
        echo.
        echo The application is ready to run without .NET installation.
        echo Simply run: dist\PortKiller.GUI.exe
        echo.
    ) else (
        echo Build failed with dotnet CLI
    )
) else (
    echo dotnet CLI not found.
    echo Please install .NET SDK from: https://dotnet.microsoft.com/download
)

pause