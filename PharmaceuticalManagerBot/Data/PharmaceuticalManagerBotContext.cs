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

            modelBuilder.Entity<MedType>(m =>
            {
                m.HasData(
                new { ID = 1, Type = "Обезболивающие и противовоспалительные" },
                new { ID = 2, Type = "Жаропонижающие" },
                new { ID = 3, Type = "Антибиотики" },
                new { ID = 4, Type = "Противовирусные" },
                new { ID = 5, Type = "Антигистаминные" },
                new { ID = 6, Type = "Сердечно-сосудистые" },
                new { ID = 7, Type = "Желудочно-кишечные" },
                new { ID = 8, Type = "Антидепрессанты и анксиолитики" },
                new { ID = 9, Type = "Гормональные препараты" },
                new { ID = 10, Type = "Витамины и минералы" },
                new { ID = 11, Type = "Противогрибковые" },
                new { ID = 12, Type = "Препараты для дыхательной системы" },
                new { ID = 13, Type = "Онкологические (противоопухолевые)" },
                new { ID = 14, Type = "Анестетики" },
                new { ID = 15, Type = "Иммуномодуляторы" },
                new { ID = 16, Type = "Спазмолитик" },
                new { ID = 17, Type = "Другое"}
                );
                m.HasMany(e => e.Medicines)
                .WithOne(e => e.MedType)
                .HasForeignKey(e => e.TypeId)
                .IsRequired();
            });

            modelBuilder.Entity<ActivePharmIngredient>()
                .HasMany(e => e.Medicines)
                .WithOne(e => e.ActivePharmIngredient)
                .HasForeignKey(e => e.ActivePharmIngredientId)
                .IsRequired();


        }
    }
}
