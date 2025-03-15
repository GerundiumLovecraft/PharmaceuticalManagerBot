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
        private readonly ITelegramBotClient _botClient;

        public DbMethods(
            ILogger<PharmaceuticalManagerBotWorker> logger,
            PharmaceuticalManagerBotContext context,
            ITelegramBotClient botClient)
        {
            _logger = logger;
            _context = context;
            _botClient = botClient;
        }

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

        public async Task AddMed (long userTgId, string med)
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
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Ошибка добавления препарата: {ex.Message}");
                throw new Exception("Произошла ошибка при добавлении препарата. Повторите ошибку чуть позже.");
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
            //put med information into the buttons and create a list with buttons
            var buttons = medPage
                .Select(m => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"{m.Name}",
                        callbackData: $"med_{m.Id}")
                })
                .ToList();
            //pagination buttons for navigation through the list
            var paginationButtons = new List<InlineKeyboardButton[]>();
            
            //adds backward button
            if (page > 0)
            {
                paginationButtons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", $"page_{page - 1}")
                });
            }
            //adds forward button
            if (medListShort.Count > (page + 1) * pageSize)
            {
                paginationButtons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Вперёд", $"page_{page + 1}")
                });
            }

            var inlineKeyboard = new InlineKeyboardMarkup(buttons.Concat(paginationButtons));

            await botClient.SendMessage(
                chatId: chatId,
                text: $"Страница {page} из {Math.Ceiling((double)(medListShort.Count / pageSize))}",
                replyMarkup: inlineKeyboard
                );
        }

        public async Task GetMedicineSoonExpireOneUser(ITelegramBotClient botClient, long chatId ,long userTgId)
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
                    string answerString = "**Срок годности истекает:**\n" + string.Join("\n", soonExpMeds.Select(m => $"- {m.Name} ({m.ExpiryDate:dd.MM.yyyy})"));

                    await botClient.SendMessage(
                        chatId: chatId,
                        text: answerString);
                }
                else
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Можете спать спокойно. Все ваши лекарства и препараты в полном порядке");
                }
            }
            else
            {
                await botClient.SendMessage(
                        chatId: chatId,
                        text: "Произошла ошибка. Пожалуйста, вбейте команду /start и повторите запрос");
                _logger.LogInformation($"Ошибка поиска по ТГ айдишке для пользователя {userTgId}");
            }

        }

        public async Task GetExpiredMed (ITelegramBotClient botClient, long chatId, long userTgId)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.TgId == userTgId);
            if (user != null)
            {
                var expMeds = await _context.Medicines
                    .AsNoTracking()
                    .Where(m => m.UserId == user.UID)
                    .Where(m => m.ExpiryDate <= today)
                    .Select(m => new MedicineDto
                    {
                        Id = m.MedicineId,
                        Name = m.Name,
                        ExpiryDate = m.ExpiryDate
                    })
                    .ToListAsync();

                if (expMeds != null && expMeds.Count > 0)
                {
                    string answerString = "**Срок годности истёк для:**\n" + string.Join("\n", expMeds.Select(m => $"- {m.Name} ({m.ExpiryDate:dd.MM.yyyy})"));

                    await botClient.SendMessage(
                        chatId: chatId,
                        text: answerString);
                }
                else
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Можете спать спокойно. В вашей аптечке нет просроченных препаратов.");
                }
            }
            else
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Ошибка при поиске информации. Повторите запрос чуть позже.");
                _logger.LogInformation($"Ошибка при поиске по ТГ айдишке пользователя {userTgId}");
            }

        }

        public async Task GetSingleMed (ITelegramBotClient botClient, long chatId, int medId)
        {
            var medInfo = await _context.Medicines
                .FindAsync(medId);
            if (medInfo != null)
            {
                string activePhar = _context.ActivePharmIngedients.Where(a => a.ID == medInfo.ActivePharmIngredientId).Select(a => a.ActivePharmIngredientName).Single();
                string medType = _context.MedTypes.Where(m => m.ID == medInfo.TypeId).Select(m => m.Type).Single();
                MedicineDto result = new MedicineDto
                {
                    Id = medInfo.MedicineId,
                    Name = medInfo.Name,
                    ActiveIngredient = activePhar,
                    Type = medType,
                    ExpiryDate = medInfo.ExpiryDate
                };
                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"Название: {result.Name}\n" + $"Активное вещество: {result.ActiveIngredient}\n" + $"Тип лекарства: {result.Type}\n" + $"Срок годности истекает: {result.ExpiryDate:dd:MM:yyyy}",
                    replyMarkup: new[] 
                    {
                        InlineKeyboardButton.WithCallbackData("закрыть", $"close_"),
                        InlineKeyboardButton.WithCallbackData("Удалить?", $"delete_{result.Id}")
                    });
            }
            else
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Извините, мы не смогли найти этот препарат.");
                _logger.LogInformation($"Проблема с поиском препарата с порядковым номером {medId}");
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
                throw new Exception("Произошла ошибка при добавлении препарата. Пожалуйста, повторите попытку чуть позже.");
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
        
        public async Task DeleteSingleMed(ITelegramBotClient botClient, long chatId, int medId)
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
                }
                else
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Произошла ошибка при поиске препарата. Пожалуйста, повторите попытку чуть позже.");
                    _logger.LogError($"Препарат под номером {medId} не был найден.");
                    
                }
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError($"Ошибка удаления препарата под номером {medId}: {ex.Message}");
                throw new Exception("Извините, я не смог удалить этот препарат. Попробуйте повторить запрос.");
            }
        }

    }
}
