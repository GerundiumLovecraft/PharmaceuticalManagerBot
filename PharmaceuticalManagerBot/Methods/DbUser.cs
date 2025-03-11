using Microsoft.EntityFrameworkCore;
using PharmaceuticalManagerBot.Data;
using PharmaceuticalManagerBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmaceuticalManagerBot.Methods
{
    internal class DbUser
    {
        private readonly PharmaceuticalManagerBotContext _context;
        public async Task<bool> ToDb (long userTgId)
        {
            try
            {
                bool userExists = await _context.Users.AnyAsync(u => u.TgId == userTgId);
                if (!userExists)
                {
                    _context.Users.Add(new User { TgId = userTgId });
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка добавления пользователя: {ex.Message}");
                return false;
            }

        }
    }
}
