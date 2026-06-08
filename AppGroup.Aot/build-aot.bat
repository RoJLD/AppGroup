@echo off
REM ============================================================================
REM  Native AOT publish script for AppGroup.Aot
REM
REM  Native AOT requires the MSVC C++ toolchain (link.exe) and its environment
REM  (INCLUDE / LIB / PATH). This script locates the latest Visual Studio (or
REM  Build Tools) install that ships the C++ tools via vswhere, initializes the
REM  x64 developer environment, then publishes the self-contained native binary.
REM
REM  Usage: just run  build-aot.bat  from anywhere (it cd's to its own folder).
REM ============================================================================
setlocal

set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
    echo [ERROR] vswhere.exe not found at "%VSWHERE%".
    echo         Install Visual Studio or the Build Tools first.
    exit /b 1
)

set "VSINSTALL="
for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do set "VSINSTALL=%%i"

if not defined VSINSTALL (
    echo [ERROR] No Visual Studio install with the C++ tools ^(VC.Tools.x86.x64^) was found.
    echo         Add the "Desktop development with C++" workload.
    exit /b 1
)

call "%VSINSTALL%\VC\Auxiliary\Build\vcvars64.bat"
if errorlevel 1 (
    echo [ERROR] Failed to initialize the MSVC x64 environment.
    exit /b 1
)

REM The ILC native linker step re-invokes vswhere.exe by name (no full path).
REM vcvars64 does not add the Installer folder to PATH, so add it explicitly,
REM otherwise "Generating native code" fails with link.exe error 123.
set "PATH=%PATH%;%ProgramFiles(x86)%\Microsoft Visual Studio\Installer"

pushd "%~dp0"
dotnet publish -c Release -r win-x64 -p:PublishAot=true -p:PublishTrimmed=true -p:SelfContained=true
set "RESULT=%errorlevel%"
popd

exit /b %RESULT%
