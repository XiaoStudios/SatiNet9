# Checklist de Verificación Pre-Migración

Antes de ejecutar las migraciones, verifica que todos estos puntos estén completados:

## ✅ CAMBIOS DE CÓDIGO

### [1] Archivo: `Sati-Net-Last.Admin/Models/UsuarioBroker.cs`
- [ ] **EXISTE** el archivo
- [ ] Contiene clase `UsuarioBroker`
- [ ] Tiene decorador `[Table("usuarios_broker")]`
- [ ] Contiene propiedades:
  - [ ] `Id` con `[Key]` y `[Column("id")]`
  - [ ] `NombreCompleto` con `[Column("nombre_completo")]` e `IsRequired`
  - [ ] `Correo` con `[Column("correo")]` e `IsRequired`
  - [ ] `ParMoneda` con `[Column("par_moneda")]` e `IsRequired`
  - [ ] `Activo` con `[Column("activo")]` y default `true`
  - [ ] `FechaRegistro` con `[Column("fecha_registro")]` y default `DateTime.UtcNow`

### [2] Archivo: `Sati-Net-Last.Admin/Data/AdminDbContext.cs`
- [ ] Contiene import `using Sati_Net_Last.Admin.Models;` (para UsuarioBroker)
- [ ] Contiene propiedad: `public DbSet<UsuarioBroker> UsuariosBroker { get; set; } = null!;`
- [ ] En método `OnModelCreating()` existe configuración para UsuarioBroker:
  ```csharp
  modelBuilder.Entity<UsuarioBroker>(e =>
  {
	  e.ToTable("usuarios_broker");
	  e.HasKey(u => u.Id);
	  e.Property(u => u.Id).HasColumnName("id");
	  e.Property(u => u.NombreCompleto).HasColumnName("nombre_completo").IsRequired();
	  e.Property(u => u.Correo).HasColumnName("correo").IsRequired();
	  e.Property(u => u.ParMoneda).HasColumnName("par_moneda").IsRequired();
	  e.Property(u => u.Activo).HasColumnName("activo");
	  e.Property(u => u.FechaRegistro).HasColumnName("fecha_registro");
  });
  ```
- [ ] No hay errores de compilación

### [3] Otros archivos (NO modificar)
- [ ] `UsuariosController.cs` - se accede a `_db.UsuariosBroker` (sin cambios)
- [ ] `Views/Usuarios/Index.cshtml` - modelo es `IEnumerable<UsuarioBroker>` (sin cambios)
- [ ] `Views/Usuarios/Create.cshtml` - modelo es `UsuarioBroker` (sin cambios)
- [ ] `Views/_ViewImports.cshtml` - contiene `@using Sati_Net_Last.Admin.Models` (sin cambios)

---

## ✅ CONFIGURACIÓN DE AMBIENTE

### [4] Dependencias .NET
- [ ] Proyecto está en **.NET 9.0** (ver `TargetFramework` en .csproj)
- [ ] Pomelo.EntityFrameworkCore.MySql versión **8.0.9+** instalado
  ```bash
  dotnet list package
  # Debería aparecer: Pomelo.EntityFrameworkCore.MySql    8.0.9
  ```

### [5] Base de Datos
- [ ] MySQL está corriendo (puerto 3306)
- [ ] Base de datos **sati_dev** existe
- [ ] Credenciales en `appsettings.json`: User=root, Password=root (verificar configuración)
- [ ] Conexión de prueba:
  ```bash
  mysql -h localhost -u root -proot -e "USE sati_dev; SELECT 1;"
  # Debería retornar: 1
  ```

### [6] Herramientas EF Core
- [ ] dotnet-ef está instalado globalmente
  ```bash
  dotnet tool list -g | grep dotnet-ef
  # Debería mostrar: dotnet-ef
  ```
- Si no está: `dotnet tool install --global dotnet-ef`

---

## ✅ COMPILACIÓN

### [7] Build exitoso
```bash
cd Sati-Net-Last.Admin
dotnet clean
dotnet build
```
- [ ] **Sin errores** de compilación
- [ ] **Sin warnings** relacionados a UsuarioBroker o AdminDbContext
- [ ] Proyecto compila correctamente

---

## ✅ PRE-MIGRACIÓN

### [8] Validar estructura del modelo
```bash
# Ejecutar desde carpeta Sati-Net-Last.Admin
dotnet ef dbcontext info
```
- [ ] Retorna información sobre AdminDbContext
- [ ] Muestra proveedor MySQL
- [ ] No hay errores

### [9] Validar que la tabla NO existe actualmente
```bash
mysql -u root -proot -e "USE sati_dev; DESC usuarios_broker;"
```
- [ ] Retorna error: "Table doesn't exist" (ESPERADO)
- O ejecutar desde MySQL:
  ```sql
  USE sati_dev;
  SHOW TABLES LIKE 'usuarios_broker';
  ```
  - [ ] Resultado vacío (tabla no existe)

---

## 🚀 PRÓXIMO PASO

Una vez completado este checklist:

**Opción A (Automatizado):**
```bash
# Windows
.\aplicar_migraciones.bat

# Linux/Mac
bash aplicar_migraciones.sh
```

**Opción B (Manual):**
```bash
cd Sati-Net-Last.Admin
dotnet ef migrations add AddUsuarioBroker
dotnet ef database update
cd ..
```

---

## ✅ POST-MIGRACIÓN

### [10] Verificar tabla creada
```bash
mysql -u root -proot -e "USE sati_dev; DESC usuarios_broker;"
```
- [ ] Tabla existe
- [ ] Columnas visibles:
  - [ ] id (INT, PK, AUTO_INCREMENT)
  - [ ] nombre_completo (VARCHAR 255, NOT NULL)
  - [ ] correo (VARCHAR 255, NOT NULL)
  - [ ] par_moneda (VARCHAR 50, NOT NULL)
  - [ ] activo (TINYINT, DEFAULT 1)
  - [ ] fecha_registro (DATETIME, DEFAULT CURRENT_TIMESTAMP)

### [11] Validar data
```sql
USE sati_dev;
SELECT COUNT(*) FROM usuarios_broker;
```
- [ ] Retorna: 0 (tabla vacía, esperado)

### [12] Prueba en aplicación
- [ ] Reinicia la aplicación (detener debugger, ejecutar de nuevo)
- [ ] Navega a: `GET /Usuarios/Index`
- [ ] **Sin excepciones**
- [ ] Muestra: "Sin clientes registrados"
- [ ] Han enlace "Crea el primer cliente aquí"
- [ ] Click en "Nuevo Cliente" → carga `GET /Usuarios/Create`
- [ ] Rellena formulario y envía `POST /Usuarios/Create`
- [ ] Redirige a Index sin errores
- [ ] Muestra cliente recién creado en tabla

---

## ⚠️ TROUBLESHOOTING

Si algo falla, verifica:

| Problema | Solución |
|----------|----------|
| `dotnet ef not found` | Instalar: `dotnet tool install --global dotnet-ef` |
| `DbContext not found` | Compilar: `dotnet build` desde carpeta Sati-Net-Last.Admin |
| `Connection refused` | MySQL no corre o credenciales incorrectas en appsettings.json |
| Tabla sigue sin existir | Ejecutar: `dotnet ef database update` sin `--dry-run` |
| Error de columnas | Verificar que OnModelCreating() mapea todos los HasColumnName() |
| Compilación falla | Verificar que UsuarioBroker.cs está en Sati-Net-Last.Admin/Models/ |

---

**Checklist completado. Proceder con migraciones.**
