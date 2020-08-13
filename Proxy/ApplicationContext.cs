using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Proxy.Models.Entities;

namespace Proxy
{
    public class ApplicationContext : DbContext
    {
        public DbSet<ProxyEntity> Proxies { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var converter = new EnumToStringConverter<ProxyType>();
            modelBuilder
                .Entity<ProxyEntity>()
                .Property(p => p.Type)
                .HasConversion(converter);

            base.OnModelCreating(modelBuilder);
        }
    }
}
