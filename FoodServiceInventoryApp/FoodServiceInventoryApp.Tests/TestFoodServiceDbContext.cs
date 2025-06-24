using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using Microsoft.EntityFrameworkCore;

namespace FoodServiceInventoryApp.Tests
{
    public class TestFoodServiceDbContext : FoodServiceDbContext
    {
        public TestFoodServiceDbContext(DbContextOptions<FoodServiceDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}