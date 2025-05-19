using Microsoft.EntityFrameworkCore;
using ToursApp.Models;

namespace ToursApp
{
    public partial class ToursContext : DbContext
    {
        private static ToursContext _instance;
        private static readonly object _lock = new object();

        private ToursContext() { }

        public static ToursContext GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new ToursContext();
                }
                return _instance;
            }
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
        public virtual DbSet<ToursApp.Models.Type> Types { get; set; }
        public virtual DbSet<TypeOfTour> TypeOfTours { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=AcerNitro\\ACERSQLEXPRESS;Database=Tours;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("PK__Country__A25C5AA6241C8430");

                entity.ToTable("Country");

                entity.Property(e => e.Code)
                    .HasMaxLength(3);

                entity.Property(e => e.Name)
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK__Hotel__3214EC07F5C2FB97");

                entity.ToTable("Hotel");

                entity.Property(e => e.CountryCode)
                    .HasMaxLength(3);

                entity.Property(e => e.Name)
                    .HasMaxLength(100);

                entity.HasOne(d => d.CountryCodeNavigation)
                    .WithMany(p => p.Hotels)
                    .HasForeignKey(d => d.CountryCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Hotel_Country");
            });

            modelBuilder.Entity<HotelComment>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK__HotelCom__3214EC073FFADED2");

                entity.ToTable("HotelComment");

                entity.Property(e => e.Author)
                    .HasMaxLength(100);

                entity.Property(e => e.CreationDate)
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Hotel)
                    .WithMany(p => p.HotelComments)
                    .HasForeignKey(d => d.HotelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HotelComment_Hotel");
            });

            modelBuilder.Entity<HotelImage>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK__HotelIma__3214EC07F632D2F6");

                entity.ToTable("HotelImage");

                entity.Property(e => e.ImageSource)
                    .HasMaxLength(255);

                entity.HasOne(d => d.Hotel)
                    .WithMany(p => p.HotelImages)
                    .HasForeignKey(d => d.HotelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HotelImage_Hotel");
            });

            modelBuilder.Entity<ToursApp.Models.Type>(entity =>
            {
                entity.HasKey(e => e.Name)
                    .HasName("PK__Type__737584F7C0F81998");

                entity.ToTable("Type");

                entity.Property(e => e.Name)
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<Tour>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK__Tour__3214EC07A7CB5FC8");

                entity.ToTable("Tour");

                entity.Property(e => e.ImagePreview)
                    .HasMaxLength(255);

                entity.Property(e => e.IsActual)
                    .HasDefaultValue(true)
                    .HasColumnName("isActual");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<TypeOfTour>(entity =>
            {
                entity.HasKey(e => new { e.TourId, e.TypeName });

                entity.ToTable("TypeOfTour");

                entity.HasOne(tot => tot.Tour)
                    .WithMany(t => t.TypeOfTours)
                    .HasForeignKey(tot => tot.TourId);

                entity.HasOne(tot => tot.Type)
                    .WithMany(t => t.TypeOfTours)
                    .HasForeignKey(tot => tot.TypeName);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
