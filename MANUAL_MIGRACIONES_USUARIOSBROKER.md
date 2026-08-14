# Instrucciones para Aplicar Migraciones - UsuarioBroker

## Resumen de Cambios Realizados

Se han realizado los siguientes cambios al código fuente:

1. ✅ **Creado:** `Sati-Net-Last.Admin\Models\UsuarioBroker.cs`
   - Clase modelo con propiedades: Id, NombreCompleto, Correo, ParMoneda, Activo, FechaRegistro
   - Decorada con atributos de mapeo a tabla `usuarios_broker` y columnas en snake_case

2. ✅ **Actualizado:** `Sati-Net-Last.Admin\Data\AdminDbContext.cs`
   - Agregado DbSet: `public DbSet<UsuarioBroker> UsuariosBroker { get; set; } = null!;`
   - Agregada configuración en `OnModelCreating()` para mapear UsuarioBroker a tabla `usuarios_broker`

## Pasos para Crear y Aplicar la Migración

### Paso 1: Instalar Herramientas de EF Core (si no está instalado)

```bash
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

### Paso 2: Navegar a la carpeta del proyecto Admin

```bash
cd "Sati-Net-Last.Admin"
```

### Paso 3: Crear la Migración

```bash
dotnet ef migrations add AddUsuarioBroker
```

**Esperado:** Esto creará una carpeta `Migrations/` con un archivo de migración que contiene:
- CREATE TABLE usuarios_broker con la estructura especificada

### Paso 4: Aplicar la Migración a la Base de Datos

```bash
dotnet ef database update
```

**Esperado:** Se ejecutará el SQL y creará la tabla `usuarios_broker` en la BD `sati_dev`

### Paso 5: Recompilar la Solución

```bash
cd ..
dotnet build
```

### Paso 6: Reiniciar la Aplicación

Detener el debugger y volver a ejecutar la aplicación. La excepción no debería ocurrir más.

## Verificación en la Base de Datos

Después de aplicar las migraciones, verifica que la tabla fue creada:

```sql
USE sati_dev;
DESC usuarios_broker;
```

**Esperado:**
```
+------------------+-----------+------+-----+---------------------+----------------+
| Field            | Type      | Null | Key | Default             | Extra          |
+------------------+-----------+------+-----+---------------------+----------------+
| id               | int       | NO   | PRI | NULL                | auto_increment |
| nombre_completo  | varchar   | NO   |     | NULL                |                |
| correo           | varchar   | NO   |     | NULL                |                |
| par_moneda       | varchar   | NO   |     | NULL                |                |
| activo           | tinyint   | YES  |     | 1                   |                |
| fecha_registro   | datetime  | YES  |     | CURRENT_TIMESTAMP   |                |
+------------------+-----------+------+-----+---------------------+----------------+
```

## Solución Alternativa (si no tienes herramientas de EF Core)

Si prefieres crear la tabla manualmente en MySQL:

```sql
USE sati_dev;

CREATE TABLE IF NOT EXISTS usuarios_broker (
  id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  nombre_completo VARCHAR(255) NOT NULL,
  correo VARCHAR(255) NOT NULL,
  par_moneda VARCHAR(50) NOT NULL,
  activo TINYINT(1) DEFAULT 1,
  fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

Luego ejecuta en la carpeta admin:
```bash
dotnet ef migrations add AddUsuarioBroker --no-transactions
```

## Solución de Problemas

### Error: "No DbContext named 'AdminDbContext' was found"
- Verifica estar en la carpeta correcta: `Sati-Net-Last.Admin/`
- Verifica que Program.cs configure AdminDbContext correctamente

### Error: "The type initializer for 'AdminDbContext' threw an exception"
- Recompila el proyecto: `dotnet build`
- Verifica la cadena de conexión en `appsettings.json`

### La tabla sigue sin existir
- Verifica que `dotnet ef database update` se ejecutó sin errores
- Revisa los logs de la base de datos MySQL para mensajes de error

---

**Con estos pasos, la excepción debe resolverse completamente.**
