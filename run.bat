@echo off
setlocal enabledelayedexpansion

echo ========================================
echo      한글 자소 정규화 도구 실행
echo ========================================
echo.

:: Check if dotnet is available
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo [ERROR] .NET Runtime이 설치되지 않았거나 PATH에 없습니다.
    echo .NET 6.0 Runtime을 설치해주세요.
    pause
    exit /b 1
)

:: Check if built executable exists
if exist "%~dp0bin\Release\net6.0-windows\HangulJasofixer.exe" (
    echo [INFO] 빌드된 실행 파일을 사용합니다...
    echo [INFO] 파일 위치: %~dp0bin\Release\net6.0-windows\HangulJasofixer.exe
    echo.
    
    start "" "%~dp0bin\Release\net6.0-windows\HangulJasofixer.exe"
    
    if %ERRORLEVEL% equ 0 (
        echo [SUCCESS] 애플리케이션이 시작되었습니다.
    ) else (
        echo [ERROR] 애플리케이션 시작에 실패했습니다.
        echo [INFO] 개발 모드로 실행을 시도합니다...
        goto :RunDev
    )
) else (
    echo [INFO] 빌드된 실행 파일이 없습니다. 개발 모드로 실행합니다...
    goto :RunDev
)

goto :End

:RunDev
echo [INFO] 개발 모드로 실행 중...
dotnet run --project HangulJasofixer.csproj
if %ERRORLEVEL% neq 0 (
    echo [ERROR] 실행에 실패했습니다.
    echo [INFO] 먼저 'build.bat'를 실행하여 프로젝트를 빌드해주세요.
)

:End
echo.
echo ========================================
pause
