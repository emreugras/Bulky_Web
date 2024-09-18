using Bulky.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BulkyWeb.DataAccess.Data
{
	public class ApplicationDbContext:IdentityDbContext<IdentityUser>
	{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base (options)
        {
            
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
		public DbSet<ShoppingCart> ShoppingCarts { get; set; }
		public DbSet<Company> Companies { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "SciFi", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 }
                );
            modelBuilder.Entity<Company>().HasData(
                new Company { Id = 1, Name = "Tech Solution", StreetAddress = "123 Tech St",City="Tech City",PostalCode="12121",State="IL",PhoneNumber="05537556306" },
                new Company { Id = 2, Name = "Denk Solution", StreetAddress = "124 Denk St", City = "Denk City", PostalCode = "12412", State = "DK", PhoneNumber = "05537556306" },
                new Company { Id = 3, Name = "Uğraş Company", StreetAddress = "134 St", City = "Ankara", PostalCode = "06200", State = "Ankara", PhoneNumber = "05537556306" }
                );
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id=1,
                    Title="Fortune of Time",
                    Author="Billy Spark",
                    Description="Praesent vitae sodales.",
                    ISBN="SWD9999001",
                    ListPrice=99,
                    Price=90,
                    Price50=85,
                    Price100=80,
                    CategoryId=1
                },
                new Product
                {
                    Id = 2,
                    Title = "Fortune of Time 2",
                    Author = "Billy Spark",
                    Description = "Praesent vitae sodales.Sodales 2",
                    ISBN = "SWD9999002",
                    ListPrice = 119,
                    Price = 115,
                    Price50 = 110,
                    Price100 = 105,
                    CategoryId = 2
                }
                );
        }
    }
}
