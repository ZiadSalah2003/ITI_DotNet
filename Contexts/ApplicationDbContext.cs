using Microsoft.EntityFrameworkCore;
using Sunrice.Models;
namespace Sunrice.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Category> Categorys { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public ApplicationDbContext(DbContextOptions options) :base(options)
        {
                
        }
   

        public DbSet<Sunrice.Models.Customer> Customer { get; set; } = default!;
    }
}
