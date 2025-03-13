using Microsoft.EntityFrameworkCore;
using PharmaceuticalManagerBot.Data;
using PharmaceuticalManagerBot.Models;
using PharmaceuticalManagerBot.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PharmaceuticalManagerBot.Methods
{
    public class DbMethods
    {
        private readonly ILogger<PharmaceuticalManagerBotWorker> _logger;
        private readonly PharmaceuticalManagerBotContext _context;

        public async Task<bool> AddUser(BigInteger userTgId, BigInteger userChatId)
        {
            try
            {
                bool userExists = await _context.Users.AnyAsync(u => u.TgId == userTgId);
                if (!userExists)
                {
                    _context.Users.Add(new Models.User { TgId = userTgId, TgChatId = userChatId });
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Логирование ошибки
                _logger.LogError($"Ошибка добавления пользователя: {ex.Message}");
                return false;
            }

        }

        public async Task<bool> AddMed (long userTgId, string med)
        {
            if (!med.Contains('|'))
            {
                throw new Exception("Пожалуйста, используйте | в качестве разделителя.");
            }
            string[] medDetails = med.Split('|')
                .Select(p => p.Trim())
                .ToArray();

            if (medDetails.Length != 4)
            {
                throw new Exception("Вы ввели не все данные");
            }

            string medName = medDetails[0];
            int userId = _context.Users.Where(u => u.TgId == userTgId).Select(u => u.UID).First();
            int medType = int.Parse(medDetails[2]);
            int medActiveId = await GetActivePharmId(medDetails[1]);
            DateOnly medExp = DateOnly.Parse(medDetails[3]);
            var newMed = new Medicine
            {
                Name = medName,
                UserId = userId,
                ActivePharmIngredientId = medActiveId,
                TypeId = medType,
                ExpiryDate = medExp
            };
            try
            {
                _context.Medicines.Add(newMed);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Ошибка добавления препарата: {ex.Message}");
                return false;
            }

        }

        public async Task<List<MedicineDto>> GetAllMedLong (long userTgId)
        {
            int userId = _context.Users.Where(u => u.TgId == userTgId).Select(u => u.UID).Single();
            var medList = await  _context.Medicines
                .Where(m => m.UserId == userId)
                .Join(_context.ActivePharmIngedients,
                m => m.ActivePharmIngredientId,
                a => a.ID,
                (m, a) => new
                {
                    ID = m.MedicineId,
                    Name = m.Name,
                    Active = a.ActivePharmIngredientName,
                    TypeId = m.TypeId,
                    ExpDate = m.ExpiryDate
                })
                .Join(_context.MedTypes,
                m => m.TypeId,
                t => t.ID,
                (m, t) => new MedicineDto
                {
                    Id = m.ID,
                    Name = m.Name,
                    ActiveIngredient = m.Active,
                    Type = t.Type,
                    ExpiryDate = m.ExpDate
                })
                .ToListAsync();
            if (medList.Count > 0)
            {
                return medList;
            }
            else
            {
                throw new Exception("Вы не добавили ещё ни одного препарата в свой список.");
            }
        }

        public async Task GetMedList (ITelegramBotClient botClient, long userTgId, long chatId, int page = 0)
        {
            const int pageSize= 5;

            int userId = _context.Users.Where(u => u.TgId == userTgId).Select(u => u.UID).Single();
            var medListShort = await _context.Medicines
                .Where(m => m.UserId == userId)
                .Select(m => new MedicineDto
                {
                    Id = m.MedicineId,
                    Name = m.Name,
                    ExpiryDate = m.ExpiryDate
                })
                .ToListAsync();

            var medPage = medListShort
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToList();
            var buttons = medPage
                .Select(m => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"{m.Name}",
                        callbackData: $"med_{m.Id}")
                })
                .ToList();
            
        }
        public async Task<MedicineDto> GetSingleMed (int medId)
        {
            var medInfo = await _context.Medicines
                .FindAsync(medId);
            if (medInfo != null)
            {
                string activePhar = _context.ActivePharmIngedients.Where(a => a.ID == medInfo.ActivePharmIngredientId).Select(a => a.ActivePharmIngredientName).Single();
                string medType = _context.MedTypes.Where(m => m.ID == medInfo.TypeId).Select(m => m.Type).Single();
                return new MedicineDto
                {
                    Id = medInfo.MedicineId,
                    Name = medInfo.Name,
                    ActiveIngredient = activePhar,
                    Type = medType,
                    ExpiryDate = medInfo.ExpiryDate
                };
            }
            else
            {
                throw new Exception("Извините, мы не смогли найти этот препарат.");
            }
        }



        private async Task<int> GetActivePharmId (string activePharmName)
        {
            bool activeExist = await _context.ActivePharmIngedients.AnyAsync(a => String.Equals(a.ActivePharmIngredientName, activePharmName, StringComparison.OrdinalIgnoreCase));
            int activeId;
            try
            {
                if (activeExist)
                {
                    activeId = _context.ActivePharmIngedients.Where(a => String.Equals(a.ActivePharmIngredientName, activePharmName, StringComparison.OrdinalIgnoreCase)).Select(a => a.ID).Single();
                }
                else
                {
                    var newActive = new ActivePharmIngredient { ActivePharmIngredientName = activePharmName };
                    activeId = newActive.ID;
                    _context.ActivePharmIngedients.Add(newActive);
                    await _context.SaveChangesAsync();
                }
                return activeId;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Ошибка добавления активного вещества: {ex.Message}");
                return 0;
            }
        }

        public string GetMedTypes()
        {
            var medTypes = _context.MedTypes.Select(m => m);
            return "**Типы лекарственных средств и их порядковый номер:**\n" + string.Join("\n", medTypes.Select(m => $"{m.ID}) {m.Type}"));
        }

        public async Task EnableAutoCheck(long userTgId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.TgId == userTgId);
                if (user != null)
                {
                    user.CheckRequired = true;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Произошёл несчастный случай. Повторите запрос чуть позже. Если не будет работать, попробуйте повторить команду /start");
                }

            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Ошибка при включении режима автоматической проверки препаратов: {ex.Message}");
                throw new Exception($"Произошла ошибка при подключении к базе данных. Повторите запрос чуть позже.");
            }
        }

        public async Task DisableAutoCheck(long userTgId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.TgId == userTgId);
                if (user != null)
                {
                    user.CheckRequired = false;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Произошёл несчастный случай. Повторите запрос чуть позже. Если не будет работать, попробуйте повторить команду /start");
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Ошибка при выключении режима автоматической проверки препаратов: {ex.Message}");
                throw new Exception($"Произошла ошибка при подключении к базе данных. Повторите запрос чуть позже.");
            }
        }

        public async Task<List<MedicineDto>> GetMedicineSoonExpireOneUser (long userTgId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var thresholdDate = today.AddDays(30);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TgId == userTgId);
            if (user != null)
            {
                var soonExpMeds = await _context.Medicines
                    .AsNoTracking()
                    .Where(m => m.UserId == user.UID)
                    .Where(m => m.ExpiryDate >= today && m.ExpiryDate <= thresholdDate)
                    .Select(m => new MedicineDto
                    {
                        Id = m.MedicineId,
                        Name = m.Name,
                        ExpiryDate = m.ExpiryDate
                    })
                    .ToListAsync();

                if (soonExpMeds != null && soonExpMeds.Count > 0)
                {
                    return soonExpMeds;
                }
                else
                {
                    throw new Exception("Можете спать спокойно. Все ваши лекарства и препараты в полном порядке");
                }
            }
            else
            {
                throw new Exception("Произошла ошибка. Пожалуйста, вбейте команду /start и повторите запрос");
            }

        }

        public async Task DeleteUser(long userTgId)
        {
            try
            {
                bool userExists = await _context.Users.AnyAsync(u => u.TgId == userTgId);

                var removeUser = await _context.Users
                        .Include(u => u.Medicines)
                        .FirstOrDefaultAsync(u => u.TgId == userTgId);
                if (removeUser != null)
                {
                    _context.Users.Remove(removeUser);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Пользователь {userTgId} и все связянные с ним записи были удалены.");
                }
                
            }
            catch (DbUpdateException ex)
            {
                // Логирование ошибки
                _logger.LogError($"Ошибка удаления пользователя: {ex.Message}");
            }
        }
        
        public async Task<bool> DeleteSingleMed(int medId)
        {
            try
            {
                bool medExists = await _context.Medicines.AnyAsync(m => m.MedicineId == medId);
                if (medExists)
                {
                    var removeMed = await _context.Medicines
                        .FirstOrDefaultAsync(m => m.MedicineId == medId);
                    _context.Medicines.Remove(removeMed);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"{removeMed.Name} удалён для пользователя {removeMed.UserId}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Препарат под номером {medId} не был найден.");
                    throw new Exception("Извините, я не смог удалить этот препарат. Попробуйте повторить запрос.");
                }
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError($"Ошибка удаления препарата под номером {medId}: {ex.Message}");
                return false;
            }
        }

    }
}
