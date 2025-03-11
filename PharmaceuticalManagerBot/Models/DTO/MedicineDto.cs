using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace PharmaceuticalManagerBot.Models.DTO
{
    public class MedicineDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? ActiveIngredient { get; set; }
        public string? Type { get; set; }
        [Required]
        public DateOnly ExpiryDate { get; set; }

    }
}
