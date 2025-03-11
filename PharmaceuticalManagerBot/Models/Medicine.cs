using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmaceuticalManagerBot.Models
{
    public class Medicine
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id")]
        public int MedicineId { get; private set; }
        [Column("user_id"), Required]
        public int UserId { get; set; }
        [Column("name"), Required, RegularExpression(@"^[A-Za-zА-Яа-я0-9\s-]{1,100}$", ErrorMessage = "Пожалуйста, используйте только кириллицу или латиницу и цифры в написании названия препарата")]
        public required string Name { get; set; }
        [Column("api_id"), Required]
        public int ActivePharmIngredientId { get; set; }
        [Column("type_id"), Required]
        public int TypeId { get; set; }
        [Column("expiry_date"), Required]
        public DateOnly ExpiryDate { get; set; }
        public User User { get; set; }
        public MedType MedType { get; set; }
        public ActivePharmIngredient ActivePharmIngredient { get; set; }
    }
}
