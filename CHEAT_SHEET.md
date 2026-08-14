# ⚡ CHEAT SHEET - Copiar & Pegar

## 🚀 INICIO RÁPIDO (30 segundos)

### Windows - Todo en Uno
```bash
.\aplicar_migraciones.bat
```

### Linux/Mac - Todo en Uno
```bash
bash aplicar_migraciones.sh
```

---

## 🔧 COMANDOS MANUALES (Si prefieres paso a paso)

### 1️⃣ Navegar a proyecto
```bash
cd Sati-Net-Last.Admin
```

### 2️⃣ Compilar
```bash
dotnet build
```

### 3️⃣ Crear migración
```bash
dotnet ef migrations add AddUsuarioBroker
```

### 4️⃣ Aplicar a BD
```bash
dotnet ef database update
```

### 5️⃣ Volver a raíz
```bash
cd ..
```

---

## ✅ VERIFICACIÓN

### ¿Compiló sin errores?
```bash
# Debería mostrar "Build succeeded"
dotnet build
```

### ¿Tabla creada en MySQL?
```bash
mysql -u root -proot sati_dev -e "DESC usuarios_broker;"
```

### ¿Estructura correcta?
Debería ver:
```
+------------------+----------+------+-----+-----+
| Field            | Type     | Null | Key | ... |
+------------------+----------+------+-----+-----+
| id               | int      | NO   | PRI | ... |
| nombre_completo  | varchar  | NO   |     | ... |
| correo           | varchar  | NO   |     | ... |
| par_moneda       | varchar  | NO   |     | ... |
| activo           | tinyint  | YES  |     | ... |
| fecha_registro   | datetime | YES  |     | ... |
+------------------+----------+------+-----+-----+
```

### ¿App funciona sin excepciones?
1. Reinicia Visual Studio
2. Click en play (Debug)
3. Navega a: `http://localhost:peuertoXXX/Usuarios/Index`
4. ✅ Debería mostrar "Sin clientes registrados"

---

## 🐛 TROUBLESHOOTING RÁPIDO

### Error: "dotnet ef: command not found"
```bash
# Solución
dotnet tool install --global dotnet-ef
```

### Error: "Table doesn't exist" (sigue ocurriendo)
```bash
# Verifica que update se ejecutó
dotnet ef database update --verbose

# O aplica manualmente el SQL:
mysql -u root -proot sati_dev < script.sql
```

### Error: Compilación falla
```bash
# Limpia y reintenta
dotnet clean
dotnet build
```

### Error: Connection refused a MySQL
```bash
# Verifica credentials en appsettings.json
cat appsettings.json | grep -A 5 "ConnectionStrings"

# Verifica que MySQL corre
mysql -u root -proot -e "SELECT 1;"
```

### Error: Migration already exists
```bash
# Elimina y recrea
dotnet ef migrations remove
dotnet ef migrations add AddUsuarioBroker
```

---

## 📊 INFO ÚTIL

### Ver migraciones pendientes
```bash
dotnet ef migrations list
```

### Ver DbContext info
```bash
dotnet ef dbcontext info
```

### Generar script SQL (sin aplicar)
```bash
dotnet ef migrations script
```

### Revertir última migración
```bash
dotnet ef migrations remove
dotnet ef database update -1
```

---

## 🔐 CREDENCIALES

### MySQL (Desarrollo)
```
Server: localhost
Port: 3306
Database: sati_dev
User: root
Password: root
```

### appsettings.json
```json
"ConnectionStrings": {
  "AdminDatabase": "Server=localhost;Port=3306;Database=sati_dev;User=root;Password=root;"
}
```

---

## 📍 UBICACIONES IMPORTANTES

```
Proyecto:          Sati-Net-Last.Admin/
Modelo:            Sati-Net-Last.Admin/Models/UsuarioBroker.cs
DbContext:         Sati-Net-Last.Admin/Data/AdminDbContext.cs
Controlador:       Sati-Net-Last.Admin/Controllers/UsuariosController.cs
Vistas:            Sati-Net-Last.Admin/Views/Usuarios/
Migraciones:       Sati-Net-Last.Admin/Migrations/
```

---

## 📚 DOCUMENTACIÓN RÁPIDA

| Necesitas | Archivo |
|-----------|---------|
| 1 página | RESUMEN_EJECUTIVO.md |
| 5 pasos | GUIA_RAPIDA.md |
| Detalles | RESUMEN_COMPLETO_CORRECCION.md |
| Verificar | CHECKLIST_PRE_MIGRACION.md |
| Preguntas | PREGUNTAS_FRECUENTES.md |

---

## 🗄️ SQL DIRECTO (Si quieres crear tabla a mano)

```sql
USE sati_dev;

CREATE TABLE IF NOT EXISTS usuarios_broker (
  id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  nombre_completo VARCHAR(255) NOT NULL,
  correo VARCHAR(255) NOT NULL,
  par_moneda VARCHAR(50) NOT NULL,
  activo TINYINT(1) DEFAULT 1,
  fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Verificar
DESC usuarios_broker;
SELECT COUNT(*) FROM usuarios_broker;
```

---

## 🔄 PIPELINE COMPLETO (Copiar & Pegar)

### Windows (.bat)
```bash
cd Sati-Net-Last.Admin && dotnet clean && dotnet build && dotnet ef migrations add AddUsuarioBroker && dotnet ef database update && cd .. && echo COMPLETO
```

### Linux/Mac (Bash)
```bash
cd Sati-Net-Last.Admin && \
dotnet clean && \
dotnet build && \
dotnet ef migrations add AddUsuarioBroker && \
dotnet ef database update && \
cd .. && \
echo "✅ COMPLETO"
```

---

## 🚀 3-PASO EXPRESS

```bash
# Paso 1
cd Sati-Net-Last.Admin && dotnet build

# Paso 2  
dotnet ef migrations add AddUsuarioBroker && dotnet ef database update

# Paso 3
cd .. && echo "✅ LISTO - Reinicia la app"
```

---

## ☑️ VALIDACIÓN 30 SEGUNDOS

```bash
# Test 1: Compilación
dotnet build                          # ← Debe pasar

# Test 2: Migración
mysql -u root -proot sati_dev -e "SHOW TABLES LIKE 'usuarios_broker';"  
# ← Debe mostrar la tabla

# Test 3: App
# Navega a http://localhost:XXXX/Usuarios/Index
# ← Debe mostrar página sin excepciones
```

---

## 📋 QUICK REFERENCE

| Tarea | Comando |
|-------|---------|
| Compilar | `dotnet build` |
| Migrar | `dotnet ef migrations add X && dotnet ef database update` |
| Ver tablas | `mysql -u root -proot sati_dev -e "SHOW TABLES;"` |
| Ver estructura | `mysql -u root -proot sati_dev -e "DESC usuarios_broker;"` |
| Limpiar migraciones | `dotnet ef migrations remove` |
| Ver migraciones | `dotnet ef migrations list` |
| Revertir | `dotnet ef database update -1` |

---

## 🎯 PUNTOS CLAVE

```
✅ COMPLETADO (Por nosotros)
  ├─ Crear UsuarioBroker.cs
  ├─ Actualizar AdminDbContext.cs
  ├─ Documentar todo
  └─ Crear scripts

⏳ TÚ DEBES HACER (10 min)
  ├─ Compilar proyecto
  ├─ Crear migración
  ├─ Aplicar a BD
  ├─ Reiniciar app
  └─ Validar

✅ RESULTADO
  └─ Sin excepciones en /Usuarios/Index
```

---

## 🎬 GUIÓN (Paso a Paso)

1. Abre Terminal/CMD
2. `cd Sati-Net-Last.Admin`
3. `dotnet clean`
4. `dotnet build`
   - ✅ Debe compilar sin errores
5. `dotnet ef migrations add AddUsuarioBroker`
   - ✅ Crea archivo en Migrations/
6. `dotnet ef database update`
   - ✅ Crea tabla en MySQL
7. `cd ..`
8. Reinicia Visual Studio Debug
9. Navega a `GET /Usuarios/Index`
   - ✅ Debe funcionar sin excepciones
10. ¡COMPLETADO! 🎉

---

## ⚡ VERSION LAZINESS-MODE (UNA LÍNEA)

### Windows
```bash
cd Sati-Net-Last.Admin && dotnet build && dotnet ef migrations add AddUsuarioBroker && dotnet ef database update && cd .. && pause
```

### Linux/Mac
```bash
cd Sati-Net-Last.Admin && dotnet build && dotnet ef migrations add AddUsuarioBroker && dotnet ef database update && cd .. && echo "✅"
```

---

## 💡 TIPS PRO

**Tip 1: Usar verbose para ver SQL**
```bash
dotnet ef database update --verbose
```

**Tip 2: Generar script sin aplicar**
```bash
dotnet ef migrations script > migracion.sql
```

**Tip 3: Ver cambios sin ejecutar**
```bash
dotnet ef database update --dry-run
```

**Tip 4: Limpiar todo y empezar
```bash
dotnet ef database update -1
dotnet ef migrations remove
# Luego repetir proceso
```

---

## 📞 AYUDA RÁPIDA

| Pregunta | Respuesta |
|----------|-----------|
| ¿Cuánto tiempo? | ~10 minutos |
| ¿Riesgo? | Muy bajo |
| ¿Reversible? | Sí, con `ef database update -1` |
| ¿Afecta otros? | No, solo crea tabla nueva |
| ¿Breaking changes? | No |

---

## 🎁 BONUS: TEMPLATE PARA FUTUROS MODELOS

Si necesitas agregar otro modelo en el futuro:

```csharp
// 1. Crear modelo
[Table("nueva_tabla")]
public class NuevaEntidad
{
	[Key][Column("id")]
	public int Id { get; set; }

	[Required][Column("nombre")]
	public string Nombre { get; set; }
}

// 2. Agregar DbSet
public DbSet<NuevaEntidad> NuevasEntidades { get; set; }

// 3. Configurar
modelBuilder.Entity<NuevaEntidad>(e =>
{
	e.ToTable("nueva_tabla");
	e.HasKey(x => x.Id);
	e.Property(x => x.Id).HasColumnName("id");
	e.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
});

// 4. Migrar
dotnet ef migrations add AddNuevaEntidad
dotnet ef database update
```

---

**Guía de referencia rápida completada.** ⚡

Copiar → Pegar → Ejecutar → ✅ Listo

