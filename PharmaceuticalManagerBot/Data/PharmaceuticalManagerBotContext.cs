using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PharmaceuticalManagerBot.Models;

namespace PharmaceuticalManagerBot.Data
{
    public class PharmaceuticalManagerBotContext : DbContext
    {
        public PharmaceuticalManagerBotContext(DbContextOptions<PharmaceuticalManagerBotContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<MedType> MedTypes { get; set; }
        public DbSet<ActivePharmIngredient> ActivePharmIngedients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("medbot");

            modelBuilder.Entity<User>()
                .HasMany(e => e.Medicines)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .IsRequired();

            modelBuilder.Entity<MedType>()
                .HasMany(e => e.Medicines)
                .WithOne(e => e.MedType)
                .HasForeignKey(e => e.TypeId)
                .IsRequired();

            modelBuilder.Entity<ActivePharmIngredient>()
                .HasMany(e => e.Medicines)
                .WithOne(e => e.ActivePharmIngredient)
                .HasForeignKey(e => e.ActivePharmIngredientId)
                .IsRequired();


        }
    }
}
