# 🔧 GUÍA RÁPIDA - Resolver Excepción UsuarioBroker

## 🎯 El Problema en 30 segundos

```
MySqlConnector.MySqlException: Table 'sati_dev.usuarios_broker' doesn't exist
```

**Causa:** La clase modelo `UsuarioBroker` no existía en el código, por lo que EF Core no podía mapear la tabla en MySQL.

---

## ✅ Lo que se ha hecho

1. ✅ **Creado:** `Sati-Net-Last.Admin/Models/UsuarioBroker.cs`
2. ✅ **Actualizado:** `Sati-Net-Last.Admin/Data/AdminDbContext.cs`
   - Agregado: `DbSet<UsuarioBroker>`
   - Agregada: Configuración en `OnModelCreating()`

---

## 🚀 Lo que DEBES hacer ahora

### PASO 1: Compilar
```bash
cd Sati-Net-Last.Admin
dotnet build
```
✅ Debe compilar sin errores

### PASO 2: Crear la migración
```bash
dotnet ef migrations add AddUsuarioBroker
```
✅ Crea carpeta `Migrations/` con archivo de migración

### PASO 3: Aplicar a BD
```bash
dotnet ef database update
```
✅ Crea tabla `usuarios_broker` en MySQL

### PASO 4: Verificar
```bash
mysql -u root -proot -e "USE sati_dev; DESC usuarios_broker;"
```
✅ Debería mostrar la estructura de la tabla

### PASO 5: Reiniciar app
- Detener debugger
- Ejecutar nuevamente
- Navegar a `/Usuarios/Index`

✅ **NO debe haber excepciones**

---

## 📋 Alternativa Automatizada

### Windows
```bash
.\aplicar_migraciones.bat
```

### Linux/Mac
```bash
bash aplicar_migraciones.sh
```

---

## 🔍 Verificación Rápida

¿Está todo bien? Verifica:

```bash
# ¿Compilación limpia?
cd Sati-Net-Last.Admin && dotnet build && cd ..

# ¿Tabla existe?
mysql -u root -proot sati_dev -e "SHOW TABLES LIKE 'usuarios_broker';"

# ¿Estructura correcta?
mysql -u root -proot sati_dev -e "DESC usuarios_broker;"

# ¿App corre sin excepciones?
# Navega a GET /Usuarios/Index
```

---

## 📁 Archivos Entregados

```
📦 Solución
├── 📄 RESUMEN_COMPLETO_CORRECCION.md     ← Explicación detallada
├── 📄 MANUAL_MIGRACIONES_USUARIOSBROKER.md ← Pasos manuales
├── 📄 CHECKLIST_PRE_MIGRACION.md         ← Verificaciones
├── 📄 GUIA_RAPIDA.md                     ← Este archivo
├── 📄 aplicar_migraciones.bat            ← Automatizar (Windows)
├── 📄 aplicar_migraciones.sh             ← Automatizar (Linux/Mac)
│
└── 📦 Sati-Net-Last.Admin
	├── 🆕 Models/UsuarioBroker.cs        ← CREADO
	├── ✏️ Data/AdminDbContext.cs         ← ACTUALIZADO
	├── Controllers/UsuariosController.cs ← (sin cambios)
	└── Views/Usuarios/               ← (sin cambios)
```

---

## ⚡ Quick Reference

| Acción | Comando |
|--------|---------|
| Ver migraciones | `dotnet ef migrations list` |
| Deshacer migración | `dotnet ef migrations remove` |
| Actualizar BD | `dotnet ef database update` |
| Ver SQL generado | `dotnet ef migrations script` |
| Diagnosticar DbContext | `dotnet ef dbcontext info` |

---

## 🆘 Si algo sale mal

**Problema:** `dotnet ef not found`
```bash
dotnet tool install --global dotnet-ef
```

**Problema:** `Connection refused`
- Verifica MySQL está corriendo
- Verifica credenciales en `appsettings.json`

**Problema:** `No DbContext found`
- Asegúrate de estar en carpeta `Sati-Net-Last.Admin`
- Compila: `dotnet build`

**Problema:** Tabla sigue sin existir
```bash
dotnet ef database update --verbose
# Verifica los logs SQL
```

---

## ✨ Validación Final

Si todo funcionó, deberías poder:

```bash
# 1. Navegar a GET /Usuarios/Index
# ✅ Lista vacía sin excepciones

# 2. Click en "Nuevo Cliente"
# ✅ Carga formulario GET /Usuarios/Create

# 3. Rellenar y enviar
# ✅ POST /Usuarios/Create → redirige a Index

# 4. Ver nuevo cliente en tabla
# ✅ Se muestra en lista
```

---

**¡Listo! La excepción debe estar resuelta.** 🎉

Para detalles adicionales, consulta `RESUMEN_COMPLETO_CORRECCION.md`.
