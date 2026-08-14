@echo off
REM Script para crear y aplicar migraciones de UsuarioBroker
REM Uso: aplicar_migraciones.bat

setlocal enabledelayedexpansion

echo.
echo ╔════════════════════════════════════════════════════════╗
echo ║  Script: Aplicar Migraciones para UsuarioBroker        ║
echo ║  Proyecto: Sati-Net-Last.Admin                         ║
echo ╚════════════════════════════════════════════════════════╝
echo.

REM Verificar que estamos en la carpeta raíz de la solución
if not exist "Sati-Net-Last.Admin\Sati-Net-Last.Admin.csproj" (
	echo [ERROR] No se encontró el archivo .csproj
	echo Por favor ejecuta este script desde la carpeta raíz de la solución
	pause
	exit /b 1
)

echo [1/7] Entrando en la carpeta del proyecto...
cd Sati-Net-Last.Admin
echo [OK] Carpeta actual: %cd%
echo.

echo [2/7] Limpiando compilación anterior...
call dotnet clean
if errorlevel 1 goto error
echo [OK] Limpieza completada
echo.

echo [3/7] Recompilando proyecto...
call dotnet build
if errorlevel 1 goto error
echo [OK] Compilación exitosa
echo.

echo [4/7] Verificando Entity Framework Tools...
dotnet tool list -g | findstr "dotnet-ef" >nul 2>&1
if errorlevel 1 (
	echo [INSTALLING] dotnet-ef no está instalado. Instalando...
	call dotnet tool install --global dotnet-ef
	if errorlevel 1 goto error
	echo [OK] dotnet-ef instalado
) else (
	echo [OK] dotnet-ef ya está instalado
	echo [UPDATING] Actualizando a versión más reciente...
	call dotnet tool update --global dotnet-ef
	if errorlevel 1 goto error
	echo [OK] dotnet-ef actualizado
)
echo.

echo [5/7] Creando migración 'AddUsuarioBroker'...
call dotnet ef migrations add AddUsuarioBroker --verbose
if errorlevel 1 goto error
echo [OK] Migración creada
echo.

echo [6/7] Aplicando migración a la base de datos...
call dotnet ef database update --verbose
if errorlevel 1 goto error
echo [OK] Migración aplicada a MySQL
echo.

REM Volver a la carpeta raíz
cd ..

echo [7/7] Proceso completado exitosamente
echo.
echo ╔════════════════════════════════════════════════════════╗
echo ║  ✅ MIGRACIONES APLICADAS CON ÉXITO                     ║
echo ║                                                        ║
echo ║  Próximos pasos:                                       ║
echo ║  1. Reinicia la aplicación                             ║
echo ║  2. Navega a /Usuarios/Index                           ║
echo ║  3. La excepción debería estar resuelta                ║
echo ║                                                        ║
echo ║  Para verificar la tabla en MySQL ejecuta:             ║
echo ║  mysql -u root -proot -e "USE sati_dev; DESC usuarios_broker;" ║
echo ╚════════════════════════════════════════════════════════╝
echo.
pause
exit /b 0

:error
echo.
echo [ERROR] Ocurrió un error durante la ejecución
echo Revisa los mensajes anteriores para más detalles
echo.
cd ..
pause
exit /b 1
