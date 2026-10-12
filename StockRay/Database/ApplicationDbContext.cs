using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockRay.Models;
using StockRay.Other;

namespace StockRay.Database
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Symbol> Symbols { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<Snapshot> Snapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");

            modelBuilder.Entity<User>()
                .HasMany(user => user.Symbols)
                .WithMany(symbol => symbol.Users);

            modelBuilder.Entity<User>()
                .HasMany(user => user.Subscriptions)
                .WithOne(subscription => subscription.User);

            modelBuilder.Entity<Symbol>()
                .HasKey(symbol => symbol.Id);

            modelBuilder.Entity<Symbol>()
                .HasMany(symbol => symbol.SnapShots)
                .WithOne(snapshot => snapshot.Symbol);

            modelBuilder.Entity<Subscription>()
                .HasKey(subscription => subscription.Id);

            modelBuilder.Entity<Snapshot>()
                .HasKey(snapshot => snapshot.Id);

            modelBuilder.Entity<Snapshot>()
                .HasIndex(snapshot => new { snapshot.SymbolId, snapshot.SnapDate })
                .IsUnique();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {


            //optionsBuilder.UseSeeding((context, _) =>
            //{

            //    var sym = context.Set<Symbol>().Any();

            //    if(!sym)
            //    {

            //        var seed = SymbolSeed.Seed();

            //        context.Set<Symbol>().AddRange(seed);
                    
            //        context.SaveChanges();
            //    }
               
            //});


        }
    }
}