using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Data;

public class DBFinanzasContext : DbContext
{
    public DBFinanzasContext(DbContextOptions<DBFinanzasContext> options)
        : base(options)
    {
    }

    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Activo> Activos => Set<Activo>();
    public DbSet<Operacion> Operaciones => Set<Operacion>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<CedearRatio> CedearRatios => Set<CedearRatio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PERSONA
        modelBuilder.Entity<Persona>(entity =>
        {
            entity.ToTable("Personas");

            entity.Property(p => p.Nombre)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(p => p.Apellido)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(p => p.Email)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.HasIndex(p => p.Email)
                  .IsUnique();
        });

        // ACTIVO
        modelBuilder.Entity<Activo>(entity =>
        {
            entity.ToTable("Activos");

            entity.Property(a => a.Symbol)
                  .HasMaxLength(30)
                  .IsRequired();

            entity.HasIndex(a => a.Symbol)
                  .IsUnique()
                  .HasDatabaseName("UX_Activos_Symbol");

            entity.Property(a => a.Nombre)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(a => a.MonedaBase)
                  .HasMaxLength(3)
                  .IsRequired()
                  .HasColumnType("char(3)");

            entity.Property(a => a.Tipo)
                  .HasConversion<byte>()        
                  .HasColumnType("tinyint")
                  .IsRequired();

            entity.Property(a => a.EsLocal)
                  .HasColumnType("bit")
                  .HasDefaultValue(false);
        });

        // OPERACION
        modelBuilder.Entity<Operacion>(entity =>
        {
            entity.ToTable("Operaciones");

            entity.Property(o => o.Tipo)
                  .HasConversion<string>()      
                  .HasMaxLength(10)
                  .HasColumnType("char(10)")
                  .IsRequired();

            entity.Property(o => o.Cantidad)
                  .HasColumnType("decimal(18,4)");

            entity.Property(o => o.PrecioUnitario)
                  .HasColumnType("decimal(18,4)");

            entity.Property(o => o.MonedaOperacion)
                  .HasMaxLength(3)
                  .HasColumnType("char(3)")
                  .IsRequired();

            entity.Property(o => o.Comision)
                  .HasColumnType("decimal(18,4)");

            entity.Property(o => o.FechaOperacion)
                  .HasColumnType("datetime2");

            entity.HasOne(o => o.Persona)
                  .WithMany(p => p.Operaciones)
                  .HasForeignKey(o => o.PersonaId)
                  .HasConstraintName("FK_Operaciones_Persona");

            entity.HasOne(o => o.Activo)
                  .WithMany(a => a.Operaciones)
                  .HasForeignKey(o => o.ActivoId)
                  .HasConstraintName("FK_Operaciones_Activo");
        });

        // COTIZACION
        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.ToTable("Cotizaciones");

            entity.Property(c => c.Precio)
                  .HasColumnType("decimal(18,4)");

            entity.Property(c => c.Moneda)
                  .HasMaxLength(3)
                  .HasColumnType("char(3)")
                  .IsRequired();

            entity.Property(c => c.TimestampUtc)
                  .HasColumnType("datetime2");

            entity.Property(c => c.Source)
                  .HasMaxLength(100);

            entity.HasOne(c => c.Activo)
                  .WithMany(a => a.Cotizaciones)
                  .HasForeignKey(c => c.ActivoId)
                  .HasConstraintName("FK_Cotizaciones_Activo");
        });

        modelBuilder.Entity<CedearRatio>(entity =>
        {
            entity.ToTable("CedearRatios");

            entity.Property(cr => cr.Ratio)
                  .HasColumnType("decimal(18,4)");

            entity.HasOne(cr => cr.Cedear)
                  .WithOne(a => a.CedearRatioCedear)
                  .HasForeignKey<CedearRatio>(cr => cr.CedearId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_CedearRatios_Cedear");

            entity.HasOne(cr => cr.UsAsset)
                  .WithOne(a => a.CedearRatioUsAsset)
                  .HasForeignKey<CedearRatio>(cr => cr.UsAssetId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_CedearRatios_UsAsset");

            entity.HasIndex(cr => new { cr.CedearId, cr.UsAssetId })
                  .IsUnique()
                  .HasDatabaseName("UX_CedearRatios");
        });
    }
}
