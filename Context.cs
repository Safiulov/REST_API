using Microsoft.EntityFrameworkCore;
using WebApplication2.DB;

namespace WebApplication2
{
    public class Context:DbContext
    {
        public Context (DbContextOptions<Context> options) : base(options)
    {
    }
        public DbSet<Spaces> Spaces{ get; set; }
        public DbSet<Tarifs> Tarifs { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<Sales> Sales { get; set; }
        public DbSet<Realisation> Realisation{ get; set; }
        public DbSet<Klients> Klients{ get; set; }
        public DbSet<Auto> Auto{ get; set; }
        
    }
}
