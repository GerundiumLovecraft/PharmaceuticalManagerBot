using PharmaceuticalManagerBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmaceuticalManagerBot.Data
{
    public static class SeedMedTypes
    {
        public static void Initialise(PharmaceuticalManagerBotContext context)
        {
            var types = new MedType[]
            {
                new ()
                {
                    Type = "Обезболивающие и противовоспалительные"
                },
                new ()
                {
                    Type = "Жаропонижающие"
                },
                new ()
                {
                    Type = "Антибиотики"
                },
                new ()
                {
                    Type = "Противовирусные"
                },
                new ()
                {
                    Type = "Антигистаминные"
                },
                new ()
                {
                    Type = "Сердечно-сосудистые"
                },
                new ()
                {
                    Type = "Желудочно-кишечные"
                },
                new ()
                {
                    Type = "Антидепрессанты и анксиолитики"
                },
                new ()
                {
                    Type = "Гормональные препараты"
                },
                new ()
                {
                    Type = "Витамины и минералы"
                },
                new ()
                {
                    Type = "Противогрибковые"
                },
                new ()
                {
                    Type = "Препараты для дыхательной системы"
                },
                new ()
                {
                    Type = "Онкологические (противоопухолевые)"
                },
                new ()
                {
                    Type = "Анестетики"
                },
                new ()
                {
                    Type = "Иммуномодуляторы"
                }
            };

            context.MedTypes.AddRange(types);
            context.SaveChanges();
        }
    }
}
