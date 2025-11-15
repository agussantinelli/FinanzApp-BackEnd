using Domain;
using Microsoft.EntityFrameworkCore;

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

    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Provincia> Provincias => Set<Provincia>();
    public DbSet<Localidad> Localidades => Set<Localidad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pais>(entity =>
        {
            entity.ToTable("Paises");

            entity.Property(p => p.Nombre)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(p => p.CodigoIso2)
                  .HasMaxLength(2)
                  .IsRequired()
                  .IsFixedLength();

            entity.Property(p => p.CodigoIso3)
                  .HasMaxLength(3)
                  .IsRequired()
                  .IsFixedLength();

            entity.Property(p => p.EsArgentina)
                  .HasColumnType("bit")
                  .HasDefaultValue(false);

            entity.HasIndex(p => p.CodigoIso2)
                  .IsUnique()
                  .HasDatabaseName("UX_Paises_Iso2");

            entity.HasIndex(p => p.CodigoIso3)
                  .IsUnique()
                  .HasDatabaseName("UX_Paises_Iso3");
        });

        modelBuilder.Entity<Provincia>(entity =>
        {
            entity.ToTable("Provincias");

            entity.Property(p => p.Nombre)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.HasOne(p => p.Pais)
                  .WithMany(pais => pais.Provincias)
                  .HasForeignKey(p => p.PaisId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Provincias_Pais");
        });

        modelBuilder.Entity<Localidad>(entity =>
        {
            entity.ToTable("Localidades");

            entity.Property(l => l.Nombre)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.HasOne(l => l.Provincia)
                  .WithMany(p => p.Localidades)
                  .HasForeignKey(l => l.ProvinciaId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Localidades_Provincia");
        });

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

            entity.Property(p => p.FechaAlta)
                  .HasColumnType("datetime2");

            entity.Property(p => p.FechaNacimiento)
                  .HasColumnType("datetime2")
                  .IsRequired();

            entity.Property(p => p.EsResidenteArgentina)
                  .HasColumnType("bit")
                  .IsRequired();

            entity.Property(p => p.Rol)
                  .HasConversion<byte>()
                  .HasColumnType("tinyint")
                  .HasDefaultValue((byte)RolPersona.Inversor)
                  .IsRequired();

            entity.HasOne(p => p.Nacionalidad)
                  .WithMany(pais => pais.PersonasNacionalidad)
                  .HasForeignKey(p => p.NacionalidadId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Personas_PaisNacionalidad");

            entity.HasOne(p => p.PaisResidencia)
                  .WithMany(pais => pais.PersonasResidencia)
                  .HasForeignKey(p => p.PaisResidenciaId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Personas_PaisResidencia");

            entity.HasOne(p => p.LocalidadResidencia)
                  .WithMany(l => l.PersonasResidencia)
                  .HasForeignKey(p => p.LocalidadResidenciaId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Personas_LocalidadResidencia");
        });

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
