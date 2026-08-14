# 🏗️ Diagrama de Arquitectura - Solución UsuarioBroker

## Antes (PROBLEMA) ❌

```
┌─────────────────────────────────────────────────────────┐
│                 ASP.NET Core App                        │
│                                                          │
│  ┌──────────────────┐                                   │
│  │ UsuariosController│                                   │
│  │                  │                                   │
│  │ _db.UsuariosBroker│◄─── INTENTA ACCEDER ────┐       │
│  └──────────────────┘                          │       │
│                                                 │       │
│  ┌──────────────────┐                          │       │
│  │  Views/Index     │                          │       │
│  │  @model          │                          │       │
│  │  UsuarioBroker◄──┘ (esperaba la clase)      │       │
│  └──────────────────┘                          │       │
│                                                 │       │
│  ┌──────────────────┐                          │       │
│  │ AdminDbContext   │                          │       │
│  │                  │                          │       │
│  │ DbSet<AdminUser> │                          │       │
│  │ DbSet<UserSymbol>│                          │       │
│  │ ❌ NO TIENE      │                          │       │
│  │ DbSet<Usuario..>◄────────────────────────────┘       │
│  └──────────────────┘                                   │
│                                                          │
└─────────────────────────────────────────────────────────┘
						 │
						 │ Intenta mapear
						 ▼
			╔════════════════════════╗
			║  MySQL: sati_dev       ║
			║                        ║
			║ ✓ Admin_Users          ║
			║ ✓ User_Symbols         ║
			║ ❌ usuarios_broker     ║
			║    (NO EXISTE)         ║
			║                        ║
			╚════════════════════════╝
					│
					▼
		MySqlConnector.MySqlException
		"Table 'sati_dev.usuarios_broker' 
		 doesn't exist"

		💥 EXCEPCIÓN EN RUNTIME
```

---

## Después (SOLUCIÓN) ✅

```
┌──────────────────────────────────────────────────────────┐
│                  ASP.NET Core App                        │
│                                                           │
│  ┌────────────────────────┐                              │
│  │ UsuariosController      │                              │
│  │                        │                              │
│  │ _db.UsuariosBroker     │◄── Accede correctamente ─┐  │
│  └────────────────────────┘                          │  │
│                                                       │  │
│  ┌────────────────────────┐                          │  │
│  │ Views/Index.cshtml     │                          │  │
│  │ @model                 │                          │  │
│  │ IEnumerable<Usuario...>│                          │  │
│  └────────────────────────┘                          │  │
│                                                       │  │
│  ┌────────────────────────┐                          │  │
│  │ Views/Create.cshtml    │                          │  │
│  │ @model UsuarioBroker   │                          │  │
│  └────────────────────────┘                          │  │
│                                                      │  │
│  ┌────────────────────────┐                         │  │
│  │ 🆕 Models/             │                         │  │
│  │    UsuarioBroker.cs    │◄────────────────────┐  │  │
│  │ ┌──────────────────┐   │                    │  │  │
│  │ │ [Table("...")]   │   │                    │  │  │
│  │ │ public class     │   │                    │  │  │
│  │ │ UsuarioBroker    │   │                    │  │  │
│  │ │ {                │   │                    │  │  │
│  │ │  Id              │   │                    │  │  │
│  │ │  NombreCompleto  │   │                    │  │  │
│  │ │  Correo          │   │                    │  │  │
│  │ │  ParMoneda       │   │                    │  │  │
│  │ │  Activo          │   │                    │  │  │
│  │ │  FechaRegistro   │   │                    │  │  │
│  │ │ }                │   │                    │  │  │
│  │ └──────────────────┘   │                    │  │  │
│  └────────────────────────┘                    │  │  │
│                                                │  │  │
│  ┌────────────────────────┐                   │  │  │
│  │ ✏️ Data/               │                   │  │  │
│  │    AdminDbContext.cs   │                   │  │  │
│  │ ┌──────────────────┐   │                   │  │  │
│  │ │ public DbSet<    │   │                   │  │  │
│  │ │ AdminUser>...    │   │                   │  │  │
│  │ │                  │   │                   │  │  │
│  │ │ public DbSet<    │   │                   │  │  │
│  │ │ UserSymbol>...   │   │                   │  │  │
│  │ │                  │   │                   │  │  │
│  │ │ 🆕 public DbSet<◄┼───┼───────────────────┘  │  │
│  │ │ UsuarioBroker>   │   │                      │  │
│  │ │ ...              │   │                      │  │
│  │ │                  │   │                      │  │
│  │ │ OnModelCreating()│   │                      │  │
│  │ │ {                │   │                      │  │
│  │ │  📋 Config       │   │                      │  │
│  │ │     Usuario...   │   │                      │  │
│  │ │ }                │   │                      │  │
│  │ └──────────────────┘   │                      │  │
│  └────────────────────────┘                      │  │
│                                                  │  │
│ Program.cs                                      │  │
│ ┌────────────────────────────────────────┐     │  │
│ │ builder.AddDbContext<AdminDbContext>()  │     │  │
│ │    .UseMySql(...)                       │     │  │
│ └────────────────────────────────────────┘     │  │
└──────────────────────────────────────────────────┬──┘
												  │
						┌─────────────────────────┘
						▼
		 ╔═══════════════════════════════╗
		 ║  EF Core Migration:           ║
		 ║  "AddUsuarioBroker"           ║
		 ║                               ║
		 ║  CREATE TABLE usuarios_broker ║
		 ║  (                            ║
		 ║    id INT PRIMARY KEY AUTO... ║
		 ║    nombre_completo VARCHAR... ║
		 ║    correo VARCHAR...          ║
		 ║    par_moneda VARCHAR...      ║
		 ║    activo TINYINT...          ║
		 ║    fecha_registro DATETIME... ║
		 ║  )                            ║
		 ╚═══════════════════════════════╝
						│
						▼
		 ╔═══════════════════════════════╗
		 ║  MySQL: sati_dev              ║
		 ║                               ║
		 ║ ✓ Admin_Users                 ║
		 ║ ✓ User_Symbols                ║
		 ║ ✅ usuarios_broker (CREADA)   ║
		 ║                               ║
		 ╚═══════════════════════════════╝
						│
						▼
			  ✅ CRUD FUNCIONAL

		 GET  /Usuarios/Index     → Lista
		 GET  /Usuarios/Create    → Formulario
		 POST /Usuarios/Create    → Insert
		 ...
```

---

## 🔄 Flujo de Resolución

```
INICIO: Excepción en GET /Usuarios/Index
   │
   ▼
┌─────────────────────────────────────┐
│ 1. ANÁLISIS (Ya completado ✓)       │
│ - Identificar causa raíz            │
│ - Definir estructura de datos       │
└──────────┬──────────────────────────┘
		   │
		   ▼
┌─────────────────────────────────────┐
│ 2. DESARROLLO (Ya completado ✓)     │
│ - Crear UsuarioBroker.cs           │
│ - Actualizar AdminDbContext         │
└──────────┬──────────────────────────┘
		   │
		   ▼
┌─────────────────────────────────────┐
│ 3. COMPILACIÓN (Tu turno 👈)        │
│ $ dotnet build                      │
│ ← Sin errores?                      │
└──────────┬──────────────────────────┘
		   │
		   ▼ SÍ
┌─────────────────────────────────────┐
│ 4. MIGRACIÓN (Tu turno 👈)          │
│ $ dotnet ef migrations add...      │
│ $ dotnet ef database update        │
│ ← Tabla creada en MySQL?            │
└──────────┬──────────────────────────┘
		   │
		   ▼ SÍ
┌─────────────────────────────────────┐
│ 5. VALIDACIÓN (Tu turno 👈)         │
│ - Reiniciar app                     │
│ - Navegar a /Usuarios/Index        │
│ ← Funciona sin excepciones?         │
└──────────┬──────────────────────────┘
		   │
		   ▼ SÍ
	   ✅ RESUELTO
```

---

## 📊 Correspondencias de Mapeo

```
MODELO C#                    BD MYSQL
─────────────────────────────────────────────────────
public class
UsuarioBroker         →   CREATE TABLE usuarios_broker

  id [Key]           →   id INT PRIMARY KEY AUTO_INCREMENT
  {                  →   (
	int Id                 id INT NOT NULL AUTO_INCREMENT
  }                  →   )

  nombre_completo    →   nombre_completo VARCHAR(255) NOT NULL
  {
	[Required]
	string
  }

  correo             →   correo VARCHAR(255) NOT NULL
  {
	[Required]
	string
  }

  par_moneda         →   par_moneda VARCHAR(50) NOT NULL
  {
	[Required]
	string
  }

  activo             →   activo TINYINT(1) DEFAULT 1
  {
	bool (default: true)
  }

  fecha_registro     →   fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP
  {
	DateTime (default: UtcNow)
  }
```

---

## 🔗 Conexión de Capas

```
┌─────────────────────────────────────┐
│    Presentation Layer (UI)          │
│  ─────────────────────────────────  │
│  • UsuariosController               │
│  • Views/Usuarios/Index.cshtml      │
│  • Views/Usuarios/Create.cshtml     │
└────────────────┬────────────────────┘
				 │ HttpRequest/Response
				 ▼
┌─────────────────────────────────────┐
│     Business Logic Layer           │
│  ─────────────────────────────────  │
│  • Validaciones                     │
│  • Lógica de negocio                │
│  • IActionResult                    │
└────────────────┬────────────────────┘
				 │ LINQ-to-SQL
				 ▼
┌─────────────────────────────────────┐
│     Data Access Layer (EF Core)    │
│  ─────────────────────────────────  │
│  • AdminDbContext                   │
│  • DbSet<UsuarioBroker>            │
│  • Migrations                       │
└────────────────┬────────────────────┘
				 │ SQL Commands
				 ▼
┌─────────────────────────────────────┐
│     Database Layer (MySQL)          │
│  ─────────────────────────────────  │
│  • usuarios_broker TABLE            │
│  • Índices                          │
│  • Datos                            │
└─────────────────────────────────────┘
```

---

## 🚀 Estado Actual

```
┌──────────────────────────────────────────────────────┐
│  Fase 1: ANÁLISIS Y DESARROLLO              ✅ DONE  │
│  ──────────────────────────────────────────────────  │
│  • Identificación de causa raíz                      │
│  • Creación de clase modelo                         │
│  • Configuración en DbContext                       │
│  • Documentación completa                           │
│                                                     │
│  Fase 2: MIGRACIONES                        ⏳ TODO  │
│  ──────────────────────────────────────────────────  │
│  • Compilar proyecto                                │
│  • Crear migración EF Core                          │
│  • Aplicar a base de datos                          │
│                                                     │
│  Fase 3: VALIDACIÓN                         ⏳ TODO  │
│  ──────────────────────────────────────────────────  │
│  • Reiniciar aplicación                             │
│  • Navegar a /Usuarios/Index                        │
│  • Crear cliente de prueba                          │
│  • Verificar sin excepciones                        │
└──────────────────────────────────────────────────────┘
```

---

**Diagrama visual completo de la solución.** 
Para ejecutar los pasos pendientes, consulta → **GUIA_RAPIDA.md**
