using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace WorkforceAPI.Data
{
    // Changed base class to IdentityDbContext
    public class EmployeeContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options)
            : base(options) { }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Required for Identity configuration

            // Configure the Employee entity
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });
        }
    }
}