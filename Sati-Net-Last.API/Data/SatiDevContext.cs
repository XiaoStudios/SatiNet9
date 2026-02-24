using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sati_Models.DTOs;

namespace Sati_Net_Last.API;

public partial class SatiDevContext : DbContext
{
    public SatiDevContext()
    {
    }

    public SatiDevContext(DbContextOptions<SatiDevContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RateDto> Rates { get; set; }

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

        modelBuilder.Entity<RateDto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rates");

            entity.Property(e => e.Close).HasColumnName("CLOSE");
            entity.Property(e => e.High).HasColumnName("HIGH");
            entity.Property(e => e.Low).HasColumnName("LOW");
            entity.Property(e => e.Open).HasColumnName("OPEN");
            entity.Property(e => e.RealVolume).HasColumnName("REAL_VOLUME");
            entity.Property(e => e.Spread).HasColumnName("SPREAD");
            entity.Property(e => e.SymbolStr).HasMaxLength(16);
            entity.Property(e => e.TickVolume).HasColumnName("TICK_VOLUME");
            entity.Property(e => e.Fecha).HasMaxLength(20).HasColumnName("FECHA");
            entity.Property(e => e.Hora).HasMaxLength(10).HasColumnName("HORA");

            entity.Property(e => e.Time)
                .HasMaxLength(20)
                .HasColumnName("TIME");

            entity.Property(e => e.Time_MT_Api)
                .HasMaxLength(20)
                .HasColumnName("Time_MT_Api");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
