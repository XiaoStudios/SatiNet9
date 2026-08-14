#!/bin/bash

# Script para crear y aplicar migraciones de UsuarioBroker
# Uso: bash aplicar_migraciones.sh

set -e  # Exit on error

echo "╔════════════════════════════════════════════════════════╗"
echo "║  Script: Aplicar Migraciones para UsuarioBroker        ║"
echo "║  Proyecto: Sati-Net-Last.Admin                         ║"
echo "╚════════════════════════════════════════════════════════╝"
echo ""

# Color codes
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Verificar que estamos en la carpeta raíz de la solución
if [ ! -f "Sati-Net-Last.Admin/Sati-Net-Last.Admin.csproj" ]; then
	echo -e "${RED}❌ Error: No se encontró el archivo .csproj${NC}"
	echo "   Por favor ejecuta este script desde la carpeta raíz de la solución"
	exit 1
fi

echo -e "${YELLOW}⏳ Paso 1: Entrando en la carpeta del proyecto...${NC}"
cd Sati-Net-Last.Admin
echo -e "${GREEN}✅ Carpeta actual: $(pwd)${NC}"
echo ""

echo -e "${YELLOW}⏳ Paso 2: Limpiando compilación anterior...${NC}"
dotnet clean
echo -e "${GREEN}✅ Limpieza completada${NC}"
echo ""

echo -e "${YELLOW}⏳ Paso 3: Recompilando proyecto...${NC}"
dotnet build
echo -e "${GREEN}✅ Compilación exitosa${NC}"
echo ""

echo -e "${YELLOW}⏳ Paso 4: Verificando Entity Framework Tools...${NC}"
if ! dotnet tool list -g | grep -q "dotnet-ef"; then
	echo -e "${YELLOW}⚠️  dotnet-ef no está instalado globalmente. Instalando...${NC}"
	dotnet tool install --global dotnet-ef
	echo -e "${GREEN}✅ dotnet-ef instalado${NC}"
else
	echo -e "${GREEN}✅ dotnet-ef ya está instalado${NC}"
	dotnet tool update --global dotnet-ef
	echo -e "${GREEN}✅ dotnet-ef actualizado a la versión más reciente${NC}"
fi
echo ""

echo -e "${YELLOW}⏳ Paso 5: Creando migración 'AddUsuarioBroker'...${NC}"
dotnet ef migrations add AddUsuarioBroker --verbose
echo -e "${GREEN}✅ Migración creada${NC}"
echo ""

echo -e "${YELLOW}⏳ Paso 6: Aplicando migración a la base de datos...${NC}"
dotnet ef database update --verbose
echo -e "${GREEN}✅ Migración aplicada a MySQL${NC}"
echo ""

echo -e "${YELLOW}⏳ Paso 7: Verificando tabla en BD...${NC}"
echo "Ejecuta el siguiente comando en MySQL:"
echo ""
echo -e "${YELLOW}mysql -u root -proot -e 'USE sati_dev; SHOW CREATE TABLE usuarios_broker;'${NC}"
echo ""

cd ..
echo "╔════════════════════════════════════════════════════════╗"
echo -e "║ ${GREEN}✅ PROCESO COMPLETADO EXITOSAMENTE${NC}               ║"
echo "║                                                        ║"
echo "║ Próximos pasos:                                        ║"
echo "║ 1. Reinicia la aplicación                              ║"
echo "║ 2. Navega a /Usuarios/Index                            ║"
echo "║ 3. La excepción debería estar resuelta                 ║"
echo "╚════════════════════════════════════════════════════════╝"
