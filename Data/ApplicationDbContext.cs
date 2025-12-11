using GokhanOzgunerWEB.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GokhanOzgunerWEB.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<AntrenorMusaitlik> AntrenorMusaitlikler { get; set; }
        public DbSet<AntrenorHizmet> AntrenorHizmetler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Salon ilişkileri
            builder.Entity<Salon>()
                .HasMany(s => s.Antrenorler)
                .WithOne(a => a.Salon)
                .HasForeignKey(a => a.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Salon>()
                .HasMany(s => s.Hizmetler)
                .WithOne(h => h.Salon)
                .HasForeignKey(h => h.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Antrenor ilişkileri
            builder.Entity<Antrenor>()
                .HasMany(a => a.Musaitlikler)
                .WithOne(m => m.Antrenor)
                .HasForeignKey(m => m.AntrenorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Antrenor>()
                .HasMany(a => a.Randevular)
                .WithOne(r => r.Antrenor)
                .HasForeignKey(r => r.AntrenorId)
                .OnDelete(DeleteBehavior.Restrict);

            // AntrenorHizmet ilişkileri (Many-to-Many)
            builder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Antrenor)
                .WithMany(a => a.AntrenorHizmetleri)
                .HasForeignKey(ah => ah.AntrenorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Hizmet)
                .WithMany(h => h.AntrenorHizmetleri)
                .HasForeignKey(ah => ah.HizmetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Randevu ilişkileri
            builder.Entity<Randevu>()
                .HasOne(r => r.Uye)
                .WithMany(u => u.Randevular)
                .HasForeignKey(r => r.UyeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Randevu>()
                .HasOne(r => r.Hizmet)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision ayarları
            builder.Entity<Hizmet>()
                .Property(h => h.Ucret)
                .HasPrecision(18, 2);

            builder.Entity<ApplicationUser>()
                .Property(u => u.Boy)
                .HasPrecision(5, 2);

            builder.Entity<ApplicationUser>()
                .Property(u => u.Kilo)
                .HasPrecision(5, 2);
        }
    }
}