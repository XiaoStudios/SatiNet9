using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sati_Models.DBModels;

namespace Sati_Net_Last.API.Data;

public partial class SatiDevContext : DbContext
{
    public SatiDevContext()
    {
    }

    public SatiDevContext(DbContextOptions<SatiDevContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminUser> AdminUsers { get; set; }

    public virtual DbSet<MtapiConnectionLog> MtapiConnectionLogs { get; set; }

    public virtual DbSet<MtapiSetting> MtapiSettings { get; set; }

    public virtual DbSet<Rate> Rates { get; set; }

    public virtual DbSet<UserSymbol> UserSymbols { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            throw new Exception("No se ha configurado el contexto de la base de datos. Utilice la sobrecarga del constructor que acepta DbContextOptions<SatiDevContext>.");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("admin_users");

            entity.HasIndex(e => e.Email, "uq_admin_users_email").IsUnique();

            entity.HasIndex(e => e.Username, "uq_admin_users_username").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsEnabled)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.LastLoginAt).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<MtapiConnectionLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("mtapi_connection_log");

            entity.HasIndex(e => e.SettingsId, "fk_mtapi_log_settings");

            entity.HasIndex(e => e.CreatedAt, "idx_mtapi_log_created");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.EventType).HasMaxLength(30);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Settings).WithMany(p => p.MtapiConnectionLogs)
                .HasForeignKey(d => d.SettingsId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_mtapi_log_settings");
        });

        modelBuilder.Entity<MtapiSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("mtapi_settings");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Host).HasMaxLength(100);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.LastConnectionAt).HasColumnType("datetime");
            entity.Property(e => e.LastConnectionStatus).HasMaxLength(20);
            entity.Property(e => e.MtPassword).HasMaxLength(255);
            entity.Property(e => e.MtUser).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Rate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rates");

            entity.HasIndex(e => new { e.Fecha, e.Hora }, "idx_rates_fecha_hora");

            entity.HasIndex(e => new { e.SymbolStr, e.Fecha, e.TimeFrame }, "idx_rates_symbol_fecha_timeframe");

            entity.HasIndex(e => new { e.SymbolStr, e.Time }, "idx_rates_symbol_time");

            entity.Property(e => e.Close).HasColumnName("CLOSE");
            entity.Property(e => e.Fecha)
                .HasMaxLength(20)
                .HasColumnName("FECHA");
            entity.Property(e => e.High).HasColumnName("HIGH");
            entity.Property(e => e.Hora)
                .HasMaxLength(10)
                .HasColumnName("HORA");
            entity.Property(e => e.Low).HasColumnName("LOW");
            entity.Property(e => e.Open).HasColumnName("OPEN");
            entity.Property(e => e.RealVolume).HasColumnName("REAL_VOLUME");
            entity.Property(e => e.Spread).HasColumnName("SPREAD");
            entity.Property(e => e.SymbolStr).HasMaxLength(16);
            entity.Property(e => e.TickVolume).HasColumnName("TICK_VOLUME");
            entity.Property(e => e.Time)
                .HasMaxLength(20)
                .HasColumnName("TIME");
            entity.Property(e => e.TimeFrame)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PERIOD_M1'");
            entity.Property(e => e.TimeMtApi)
                .HasMaxLength(20)
                .HasColumnName("Time_MT_Api");
        });

        modelBuilder.Entity<UserSymbol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user_symbols");

            entity.HasIndex(e => e.UserId, "idx_user_symbols_user");

            entity.HasIndex(e => e.SymbolStr, "uq_user_symbols_symbol").IsUnique();

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.SymbolStr).HasMaxLength(16);

            entity.HasOne(d => d.User).WithMany(p => p.UserSymbols)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user_symbols_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
