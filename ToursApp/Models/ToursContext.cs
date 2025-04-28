using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ToursApp.Models;

public partial class ToursContext : DbContext
{
    private static ToursContext _instance;

    // Приватный конструктор
    private ToursContext() {}

    public static ToursContext GetInstance()
    {
        if (_instance == null)
        {
            _instance = new ToursContext();
        }
        return _instance;
    }
    public ToursContext(DbContextOptions<ToursContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Hotel> Hotels { get; set; }

    public virtual DbSet<HotelComment> HotelComments { get; set; }

    public virtual DbSet<HotelImage> HotelImages { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<Type> Types { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=AcerNitro\\ACERSQLEXPRESS;Database=Tours;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PK__Country__A25C5AA6241C8430");

            entity.ToTable("Country");

            entity.Property(e => e.Code).HasMaxLength(3);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Hotel__3214EC07F5C2FB97");

            entity.ToTable("Hotel");

            entity.Property(e => e.CountryCode).HasMaxLength(3);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.CountryCodeNavigation).WithMany(p => p.Hotels)
                .HasForeignKey(d => d.CountryCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Hotel_Country");
        });

        modelBuilder.Entity<HotelComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HotelCom__3214EC073FFADED2");

            entity.ToTable("HotelComment");

            entity.Property(e => e.Author).HasMaxLength(100);
            entity.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Hotel).WithMany(p => p.HotelComments)
                .HasForeignKey(d => d.HotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HotelComment_Hotel");
        });

        modelBuilder.Entity<HotelImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HotelIma__3214EC07F632D2F6");

            entity.ToTable("HotelImage");

            entity.Property(e => e.ImageSource).HasMaxLength(255);

            entity.HasOne(d => d.Hotel).WithMany(p => p.HotelImages)
                .HasForeignKey(d => d.HotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HotelImage_Hotel");
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tour__3214EC07A7CB5FC8");

            entity.ToTable("Tour");

            entity.Property(e => e.ImagePreview).HasMaxLength(255);
            entity.Property(e => e.IsActual)
                .HasDefaultValue(true)
                .HasColumnName("isActual");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

            entity.HasMany(d => d.TypeNames).WithMany(p => p.Tours)
                .UsingEntity<Dictionary<string, object>>(
                    "TypeOfTour",
                    r => r.HasOne<Type>().WithMany()
                        .HasForeignKey("TypeName")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TypeOfTour_Type"),
                    l => l.HasOne<Tour>().WithMany()
                        .HasForeignKey("TourId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TypeOfTour_Tour"),
                    j =>
                    {
                        j.HasKey("TourId", "TypeName").HasName("PK__TypeOfTo__ED0297CAA363CB9A");
                        j.ToTable("TypeOfTour");
                        j.IndexerProperty<string>("TypeName").HasMaxLength(50);
                    });
        });

        modelBuilder.Entity<Type>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PK__Type__737584F7C0F81998");

            entity.ToTable("Type");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}