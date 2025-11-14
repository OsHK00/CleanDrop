@echo off
title Build CleanDrop - Release

echo ===============================
echo   Limpieza de compilaciones
echo ===============================
dotnet clean

echo.
echo ===============================
echo   Eliminando carpetas bin/ y obj/
echo ===============================

for /d /r %%i in (bin,obj) do (
    echo Eliminando %%i
    rd /s /q "%%i"
)

echo.
echo ===============================
echo   Publicando CleanDrop.Console
echo ===============================
dotnet publish CleanDrop.Console ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -o dist\Console

echo.
echo ===============================
echo   Publicando TrayApp
echo ===============================
dotnet publish TrayApp ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -o dist\TrayApp

echo.
echo ===============================
echo   Preparando carpeta final /dist
echo ===============================
mkdir dist\final >nul 2>nul

copy /y dist\Console\*.exe dist\final\
copy /y dist\TrayApp\*.exe dist\final\

echo.
echo ===============================
echo   Eliminando archivos PDB
echo ===============================
del /q dist\final\*.pdb >nul 2>nul

echo.
echo ===============================
echo   COMPILACIÓN COMPLETA
echo ===============================
echo Ejecutables finales en: dist\final\
echo.
pause
