using Microsoft.EntityFrameworkCore;
using StockRay.Models;
using StockRay.Other;

namespace StockRay.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options): base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<Symbol> Symbols { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<Snapshot> Snapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);


            modelBuilder.Entity<User>()
                .HasMany(u => u.Symbols)
                .WithMany(s => s.Users);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Subscriptions)
                .WithOne(s => s.User);

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.Email, u.UserName })
                .IsUnique();

            modelBuilder.Entity<Symbol>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Symbol>()
                .HasMany(s => s.SnapShots)
                .WithOne(sn => sn.Symbol);


            modelBuilder.Entity<Subscription>()
                .HasKey(sp => sp.Id);

            modelBuilder.Entity<Snapshot>()
                .HasKey(sn => sn.Id);

            modelBuilder.Entity<Snapshot>()
                .HasIndex(sn => new {sn.SymbolId, sn.SnapDate})
                .IsUnique();



            
                
            
                
            
          
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {


            optionsBuilder.UseSeeding((context, _) =>
            {

                var sym = context.Set<Symbol>().Any();

                if(!sym)
                {

                    var seed = SymbolSeed.Seed();

                    context.Set<Symbol>().AddRange(seed);
                    
                    context.SaveChanges();
                }
               
            });
        }
    }
}
