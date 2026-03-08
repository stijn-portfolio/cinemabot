using Cinema.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Data;

public class CinemaDbContext : DbContext
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Screening> Screenings { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Movie configuration
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Genre).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Rating).HasColumnType("decimal(3,1)");
            entity.Property(e => e.PosterUrl).IsRequired().HasMaxLength(500);

            // One-to-many relationship: Movie -> Screenings
            entity.HasMany(e => e.Screenings)
                .WithOne(s => s.Movie)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Screening configuration
        modelBuilder.Entity<Screening>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Room).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");

            // One-to-many relationship: Screening -> Bookings
            entity.HasMany(e => e.Bookings)
                .WithOne(b => b.Screening)
                .HasForeignKey(b => b.ScreeningId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Booking configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BookingCode).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.BookingCode).IsUnique();
        });
    }
}
