using testAPI.api.domain.Entities;
using testAPI.api.infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace testAPI.api.infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Authority> Authorities { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<OperationKey> OperationKeys { get; set; }
        public DbSet<Signature> Signatures { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<OTP> OTPs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.Property(e => e.CreatedOn).IsRequired();
                entity.Property(e => e.ExpiresOn).IsRequired();

                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            builder.Entity<History>()
                .HasOne<AppUser>()
                .WithMany(u => u.Histories)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Signature>()
                .HasOne<AppUser>()
                .WithMany(u => u.Signatures)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(true);

            builder.Entity<Signature>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt);

                entity.HasOne<AppUser>()
                    .WithMany(u => u.Signatures)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(true);
            });

            builder.Entity<OTP>()
                .HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OTP>()
                .HasIndex(o => new { o.UserId, o.PhoneNumber, o.CreatedAt })
                .IsUnique(false);
        }
    }
}
