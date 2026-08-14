# ❓ Preguntas Frecuentes - Excepción UsuarioBroker

## ¿Por qué ocurrió esta excepción?

La clase modelo `UsuarioBroker` no estaba definida en el código fuente, pero el controlador y la vista la referenciaban. Cuando EF Core intentó acceder a `_db.UsuariosBroker`, generó una consulta SQL para una tabla inexistente `usuarios_broker` en MySQL, causando la excepción.

---

## ¿Qué archivos se modificaron?

Solo se crearon/modificaron 2 archivos:

1. **CREADO:** `Sati-Net-Last.Admin/Models/UsuarioBroker.cs`
   - Clase modelo con propiedades necesarias
   - Decorada con atributos de mapeo EF Core

2. **ACTUALIZADO:** `Sati-Net-Last.Admin/Data/AdminDbContext.cs`
   - Agregado DbSet<UsuarioBroker>
   - Agregada configuración en OnModelCreating()

Todos los demás archivos quedan sin cambios.

---

## ¿Necesito hacer cambios manuales en la BD?

**No es obligatorio**, pero tienes dos opciones:

**Opción A (RECOMENDADA):** Usar EF Core Migrations
- Mantiene sincronización automática entre código y BD
- Genera scripts SQL versionados
- Más profesional para producción

**Opción B:** Script SQL manual
- Ejecutar el SQL directamente en MySQL
- Útil si no tienes acceso a herramientas EF

Ambas resultan en la misma tabla.

---

## ¿Qué hace dotnet ef database update?

Ejecuta todos los scripts SQL de las migraciones pendientes contra la base de datos. En este caso, crea la tabla `usuarios_broker` con la estructura especificada.

---

## ¿Puedo rescindir los cambios?

**Sí**, hay dos formas:

**Si ya aplicaste la migración:**
```bash
cd Sati-Net-Last.Admin
dotnet ef database update -1  # Revierte la última migración
dotnet ef migrations remove   # Elimina el archivo de migración
```

**Si aún no aplicaste:**
```bash
cd Sati-Net-Last.Admin
dotnet ef migrations remove   # Solo elimina el archivo
```

Después elimina los archivos creados:
- `Sati-Net-Last.Admin/Models/UsuarioBroker.cs`
- Revertir cambios en `AdminDbContext.cs`

---

## ¿Qué pasa si la migración falla?

**En logs verás:** Error SQL detallado

**Causas comunes:**
- MySQL no está corriendo
- Credenciales incorrectas en `appsettings.json`
- Tabla ya existe (si ejecutaste el script manual antes)

**Solución:**
```bash
# Ver el error en detalle
dotnet ef database update --verbose

# Verificar BD
mysql -u root -proot -e "USE sati_dev; SHOW TABLES;"

# Si la tabla existe, puedes:
# 1. Eliminarla manualmente y reintentar
# 2. O ejecutar: dotnet ef migrations remove, luego recrear
```

---

## ¿Necesito crear índices en la tabla?

EF Core crea solo lo mínimo: PK y columnas con mapeo. Para producción, podrías agregar:

```sql
-- Índice para búsquedas por correo
ALTER TABLE usuarios_broker ADD UNIQUE INDEX idx_correo (correo);

-- Índice para ordenar por fecha
ALTER TABLE usuarios_broker ADD INDEX idx_fecha_registro (fecha_registro DESC);

-- Índice para filtrar por estado
ALTER TABLE usuarios_broker ADD INDEX idx_activo (activo);
```

Esto optimiza queries pero no es obligatorio para que funcione.

---

## ¿Puedo personalizar el nombre de la tabla?

**Sí**, en `AdminDbContext.cs`, línea con `e.ToTable("usuarios_broker")`:

```csharp
// Para cambiar de "usuarios_broker" a otro nombre:
e.ToTable("nombre_tabla_personalizado");
```

Luego ejecuta:
```bash
dotnet ef migrations add EditarNombreTabla
dotnet ef database update
```

---

## ¿La contraseña root=root es segura?

**No, de hecho**, esto está en desarrollo. Para producción:

1. Cambia credenciales en `appsettings.json`
2. Usa valores desde variables de entorno
3. Ejemplo seguro:
```json
{
  "ConnectionStrings": {
	"AdminDatabase": "Server=db.prod.com;Port=3306;Database=sati_prod;User=app_user;Password=${DB_PASSWORD};"
  }
}
```

El valor `${DB_PASSWORD}` se inyecta en tiempo de ejecución desde variable de entorno.

---

## ¿Puedo agregar más campos a UsuarioBroker después?

**Sí, es fácil:**

1. Agrega propiedad a `UsuarioBroker.cs`:
```csharp
[Column("telefono")]
[StringLength(20)]
public string? Telefono { get; set; }
```

2. Agrega mapeo en `AdminDbContext.cs`:
```csharp
e.Property(u => u.Telefono).HasColumnName("telefono");
```

3. Crea nueva migración:
```bash
dotnet ef migrations add AddTelefonoToUsuarioBroker
dotnet ef database update
```

---

## ¿Qué sucede si dejo la app corriendo mientras aplico migraciones?

**Puede haber conflictos:**
- Transacciones abiertas podrían bloquear el alter table
- Mejor: Detener la app, aplicar migraciones, reiniciar

---

## ¿Las vistas se actualizarán automáticamente?

**Sí**:
- `Index.cshtml` ya espera `IEnumerable<UsuarioBroker>` ✅
- `Create.cshtml` ya tiene los campos correctos ✅
- Ninguna vista necesita cambios

---

## ¿Hay que hacer backup de la BD antes?

**No es obligatorio**, pero es buena práctica:

```bash
# Backup completo
mysqldump -u root -proot sati_dev > backup_sati_dev_$(date +%Y%m%d).sql

# Importar backup si algo falla
mysql -u root -proot sati_dev < backup_sati_dev_20240115.sql
```

---

## ¿El DbSet<UsuarioBroker> necesita inicialización en OnConfiguring?

**No**, porque está configurado en Program.cs:
```csharp
builder.Services.AddDbContext<AdminDbContext>(options =>
	options.UseMySql(adminConn, ServerVersion.AutoDetect(adminConn)));
```

EF Core maneja la inicialización automáticamente.

---

## ¿Puedo usar Lazy Loading o Eager Loading?

**Sí**, pero UsuarioBroker no tiene relaciones actualmente. Si la necesita después:

```csharp
// Eager Loading en Index()
var usuarios = await _db.UsuariosBroker
	.Include(u => u.SuNavegacion)  // Si tuviera relación
	.OrderByDescending(u => u.FechaRegistro)
	.ToListAsync();
```

---

## ¿Necesito crear Repository Pattern para UsuarioBroker?

**No es obligatorio**, pero es una buena práctica. Actualmente:
- El controlador accede directamente a `_db.UsuariosBroker`
- Para producción, considera un repositorio intermediario

Ejemplo futuro:
```csharp
public interface IUsuarioBrokerRepository
{
	Task<List<UsuarioBroker>> GetAllAsync();
	Task<UsuarioBroker> GetByIdAsync(int id);
	Task AddAsync(UsuarioBroker usuario);
}
```

---

## ¿Esta solución funciona en .NET 9.0?

**Sí, perfectamente**. El proyecto ya está en .NET 9.0 y Pomelo.EntityFrameworkCore.MySql 8.0.9 es compatible.

---

## Documentación de Referencia

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Pomelo MySQL Provider](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
- [Data Annotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)

---

**¿Más preguntas? Revisa los otros documentos:**
- `RESUMEN_COMPLETO_CORRECCION.md` - Análisis técnico profundo
- `GUIA_RAPIDA.md` - Pasos rápidos
- `CHECKLIST_PRE_MIGRACION.md` - Verificaciones antes de empezar
