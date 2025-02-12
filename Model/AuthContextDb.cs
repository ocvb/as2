using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace _234412H_AS2.Model
{
    public class AuthContextDb(DbContextOptions<AuthContextDb> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<AuditLog> AuditLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
