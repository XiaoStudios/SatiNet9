using Microsoft.EntityFrameworkCore;
using Sati_Net_Last.Admin.Models;

namespace Sati_Net_Last.Admin.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

    public DbSet<AdminUser> Admins { get; set; } = null!;
    public DbSet<UserSymbol> UserSymbols { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUser>(e =>
        {
            e.ToTable("Admin_Users");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("Id");
            e.Property(a => a.Username).HasColumnName("Username");
            e.Property(a => a.Email).HasColumnName("Email");
            e.Property(a => a.PasswordHash).HasColumnName("PasswordHash");
            e.Property(a => a.FullName).HasColumnName("FullName");
            e.Property(a => a.IsEnabled).HasColumnName("IsEnabled");
            e.Property(a => a.IsAdmin).HasColumnName("IsAdmin");
        });

        modelBuilder.Entity<UserSymbol>(e =>
        {
            e.ToTable("User_Symbols");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("Id");
            e.Property(u => u.UserId).HasColumnName("UserId");
            e.Property(u => u.SymbolStr).HasColumnName("SymbolStr");
            e.Property(u => u.IsActive).HasColumnName("IsActive");
            e.Property(u => u.AssignedAt).HasColumnName("AssignedAt");

            // Relación: 1 Usuario tiene símbolos asignados
            e.HasOne(u => u.User)
             .WithMany()
             .HasForeignKey(u => u.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}