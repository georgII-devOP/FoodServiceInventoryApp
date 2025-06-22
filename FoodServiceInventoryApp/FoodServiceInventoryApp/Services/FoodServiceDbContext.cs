using Microsoft.EntityFrameworkCore;
using FoodServiceInventoryApp.Models;
using BCrypt.Net;

namespace FoodServiceInventoryApp.Services
{
    public class FoodServiceDbContext : DbContext
    {
        public FoodServiceDbContext(DbContextOptions<FoodServiceDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSupplyHistory> ProductSupplyHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Supplier>().ToTable("Suppliers");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<ProductSupplyHistory>().ToTable("ProductSupplyHistory");

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Продукты питания" },
                new Category { CategoryId = 2, CategoryName = "Посуда и однораз. упаковка" },
                new Category { CategoryId = 3, CategoryName = "Оборудование и инвентарь" },
                new Category { CategoryId = 4, CategoryName = "Расходные материалы для бизнеса" },
                new Category { CategoryId = 5, CategoryName = "Чистящие и дезинфицирующие средства" }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}