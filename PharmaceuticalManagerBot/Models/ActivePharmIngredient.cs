using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PharmaceuticalManagerBot.Models
{
    public class ActivePharmIngredient
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        [Column("api"), Required, RegularExpression(@"^[A-Za-zА-Яа-я0-9\s-]{1,100}$", ErrorMessage = "Пожалуйста, используйте только кириллицу или латиницу и цифры в написании названия препарата")]
        public string ActivePharmIngredientName { get; set; }
        public List<Medicine>? Medicines { get; set; }
    }
}
