using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace _234412H_AS2.Model
{
    public class AuthContextDb : IdentityDbContext<ApplicationUser>
    {
        public AuthContextDb(DbContextOptions<AuthContextDb> options) : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<TwoFactorAuth> TwoFactorAuths { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TwoFactorAuth>()
                .HasOne(t => t.User)
                .WithOne()
                .HasForeignKey<TwoFactorAuth>(t => t.UserId);

            modelBuilder.Entity<PasswordHistory>()
                .HasOne(ph => ph.User)
                .WithMany()
                .HasForeignKey(ph => ph.UserId);
        }
    }
}
