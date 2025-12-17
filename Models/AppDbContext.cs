using Microsoft.EntityFrameworkCore;

namespace WebbSystem.NET.Models
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Theo Wikström",

            }
            );
        }

    }
}