using ExchangeRatesWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRatesWebApp.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<ForumComment> ForumComments { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
