using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace WorkforceAPI.Data
{
    public class EmployeeContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options)
            : base(options)
        {
        }

       
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<HRUser> HRUsers { get; set; }
        public DbSet<WorkforceUser> WorkforceUsers { get; set; }

        
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectEmployee> ProjectEmployees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureEmployeeModel(modelBuilder);
            ConfigureUserModels(modelBuilder);
            ConfigureProjectModels(modelBuilder);
        }

        private void ConfigureEmployeeModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeID);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.EmployeeName)
                    .IsRequired()
                    .HasMaxLength(100);
            });
        }

        private void ConfigureUserModels(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<HRUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<WorkforceUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            });
        }

        private void ConfigureProjectModels(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.Id);  

                entity.Property(p => p.Name) 
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending");

                entity.HasIndex(p => p.Name)
                    .IsUnique();

                entity.Property(p => p.StartDate)
                    .IsRequired();

                entity.Property(p => p.EndDate)
                    .IsRequired();

                
                entity.OwnsMany(p => p.Employees, e =>
                {
                    e.WithOwner().HasForeignKey("ProjectId");
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.HasKey("Id");
                });
            });

        }
    }
}