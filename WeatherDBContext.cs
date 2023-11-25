using Microsoft.EntityFrameworkCore;

namespace net.zmau.weatherarchiver
{
    internal class WeatherDBContext : DbContext
    {
        public DbSet<WeatherItem> Weather { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=localhost;Database=weatherDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
