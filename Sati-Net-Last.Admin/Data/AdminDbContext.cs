using Microsoft.EntityFrameworkCore;
using Sati_Net_Last.Admin.Models;

namespace Sati_Net_Last.Admin.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<AdminUser> Admins { get; set; } = null!;
    public DbSet<UsuarioBroker> UsuariosBroker { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Configuración exacta de la tabla ROLES
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("roles"); // En minúsculas y en plural, tal cual está en MySQL
            e.HasKey(r => r.Id);

            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.TipoRol).HasColumnName("tiporol").IsRequired().HasMaxLength(100);
        });

        // 2. Configuración exacta de la tabla ADMIN (El error estaba aquí)
        modelBuilder.Entity<AdminUser>(e =>
        {
            e.ToTable("admin"); // CORREGIDO: "admin" sin la 's' final
            e.HasKey(a => a.Id);

            // Mapeo estricto de columnas para asegurar 100% de compatibilidad con MySQL
            e.Property(a => a.Id).HasColumnName("id");
            e.Property(a => a.NombreAdmin).HasColumnName("nombre_admin").IsRequired().HasMaxLength(200);
            e.Property(a => a.Correo).HasColumnName("correo").IsRequired().HasMaxLength(200);
            e.Property(a => a.PasswordHash).HasColumnName("password").IsRequired().HasMaxLength(200);
            e.Property(a => a.RolId).HasColumnName("rol_id");

            // Configuración de la llave foránea
            e.HasOne(a => a.Rol)
             .WithMany()
             .HasForeignKey(a => a.RolId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // 3. Configuración exacta de la tabla USUARIOS_BROKER
        modelBuilder.Entity<UsuarioBroker>(e =>
        {
            e.ToTable("usuarios_broker");
            e.HasKey(u => u.Id);

            // Mapeo estricto de columnas para compatibilidad con MySQL
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.NombreCompleto).HasColumnName("nombre_completo").IsRequired().HasMaxLength(200);
            e.Property(u => u.Correo).HasColumnName("correo").IsRequired().HasMaxLength(200);
            e.Property(u => u.ParMoneda).HasColumnName("par_moneda").IsRequired().HasMaxLength(50);
            e.Property(u => u.Activo).HasColumnName("activo");
            e.Property(u => u.FechaRegistro).HasColumnName("fecha_registro");
        });
    }
}