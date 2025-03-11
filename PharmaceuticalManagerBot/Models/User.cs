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
    [Index(nameof(UID), IsUnique = true), Index(nameof(TgId), IsUnique = true)]
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("uid")]
        public int UID { get; private set; }
        [Required, Column("telegram_id")]
        public BigInteger TgId { get; set; }
        [Required, Column("telegram_chat_id")]
        public BigInteger TgChatId { get; set; }
        [Column("check_req")]
        public bool CheckRequired { get; set; } = false;
        public List<Medicine>? Medicines { get; set; }
    }
}
