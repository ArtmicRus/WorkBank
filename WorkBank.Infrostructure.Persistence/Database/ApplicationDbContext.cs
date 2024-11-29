using Microsoft.EntityFrameworkCore;
using WorkBank.Database.Configurations;
using WorkBank.Models;

namespace WorkBank.Database
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Person> Persons => Set<Person>();

        public DbSet<Passport> Passports => Set<Passport>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=WorkKursDatabase;Username=worker;Password=worker");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new PassportEntityTypeConfiguration().Configure(modelBuilder.Entity<Passport>());
            new PersonEntityTypeConfiguration().Configure(modelBuilder.Entity<Person>());
        }
    }

}