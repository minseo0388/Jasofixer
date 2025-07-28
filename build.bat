@echo off
setlocal enabledelayedexpansion

echo ========================================
echo       한글 자소 정규화 도구 빌드
echo ========================================
echo.

:: Check if dotnet is available
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo [ERROR] .NET SDK가 설치되지 않았거나 PATH에 없습니다.
    echo .NET 6.0 SDK를 설치해주세요.
    pause
    exit /b 1
)

echo [INFO] .NET SDK 버전:
dotnet --version
echo.

:: Clean previous builds
echo [INFO] 이전 빌드 정리 중...
if exist "bin" rmdir /s /q "bin" 2>nul
if exist "obj" rmdir /s /q "obj" 2>nul
echo [INFO] 정리 완료.
echo.

:: Restore dependencies
echo [INFO] 종속성 복원 중...
dotnet restore HangulJasofixer.csproj
if %ERRORLEVEL% neq 0 (
    echo [ERROR] 종속성 복원에 실패했습니다.
    pause
    exit /b 1
)
echo [INFO] 종속성 복원 완료.
echo.

:: Build the project
echo [INFO] 프로젝트 빌드 중...
dotnet build HangulJasofixer.csproj --configuration Release --no-restore
if %ERRORLEVEL% equ 0 (
    echo.
    echo [SUCCESS] 빌드 성공!
    echo [INFO] 실행 파일 위치: %~dp0bin\Release\net6.0-windows\HangulJasofixer.exe
    echo.
    
    :: Check if executable exists
    if exist "%~dp0bin\Release\net6.0-windows\HangulJasofixer.exe" (
        echo [INFO] 실행 파일이 정상적으로 생성되었습니다.
        
        :: Optional: Show file size
        for %%A in ("%~dp0bin\Release\net6.0-windows\HangulJasofixer.exe") do (
            echo [INFO] 파일 크기: %%~zA bytes
        )
    ) else (
        echo [WARNING] 실행 파일을 찾을 수 없습니다.
    )
) else (
    echo.
    echo [ERROR] 빌드 실패!
    echo [INFO] 오류 내용을 확인하고 다시 시도해주세요.
)

echo.
echo ========================================
pause
