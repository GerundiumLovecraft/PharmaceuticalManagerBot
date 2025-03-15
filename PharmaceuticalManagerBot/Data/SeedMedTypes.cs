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
                new () { ID = 1, Type = "Обезболивающие и противовоспалительные" },
                new () { ID = 2, Type = "Жаропонижающие" },
                new () { ID = 3, Type = "Антибиотики" },
                new () { ID = 4, Type = "Противовирусные" },
                new () { ID = 5, Type = "Антигистаминные" },
                new () { ID = 6, Type = "Сердечно-сосудистые" },
                new () { ID = 7, Type = "Желудочно-кишечные" },
                new () { ID = 8, Type = "Антидепрессанты и анксиолитики" },
                new () { ID = 9, Type = "Гормональные препараты" },
                new () { ID = 10, Type = "Витамины и минералы" },
                new () { ID = 11, Type = "Противогрибковые" },
                new () { ID = 12, Type = "Препараты для дыхательной системы" },
                new () { ID = 13, Type = "Онкологические (противоопухолевые)" },
                new () { ID = 14, Type = "Анестетики" },
                new () { ID = 15, Type = "Иммуномодуляторы" },
                new () { ID = 16, Type = "Спазмолитик"},
                new () { ID = 17, Type = "Другое"}
            };

            context.MedTypes.AddRange(types);
            context.SaveChanges();
        }
    }
}
