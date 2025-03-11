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
    public class MedType
    {
        [Column("id"), Key]
        public int ID { get; set; }
        [Column("type"), Required, RegularExpression(@"^[A-Za-zА-Яа-я0-9\s-]{1,100}$")]
        public string Type { get; set; }
        public List<Medicine>? Medicines { get; set; }
    }
}
