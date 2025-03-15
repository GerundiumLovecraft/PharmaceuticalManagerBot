using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using PharmaceuticalManagerBot.Data;
using PharmaceuticalManagerBot.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace PharmaceuticalManagerBot
{
    public class ExpiryNotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiryNotificationService> _logger;
        private readonly ITelegramBotClient _botClient;

        public ExpiryNotificationService(
            IServiceScopeFactory scopeFactory,
            ILogger<ExpiryNotificationService> logger,
            ITelegramBotClient botClient)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _botClient = botClient;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndNotifyAsync();
                    _logger.LogInformation("Проверка сроков годности завершена");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при проверке сроков годности");
                }


                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

        }

        private async Task CheckAndNotifyAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<PharmaceuticalManagerBotContext>();
                //Methods to interract with DB
                var _dbMethods = scope.ServiceProvider.GetRequiredService<DbMethods>();
                var today = DateOnly.FromDateTime(DateTime.Today);
                //30 day threshold for the soon to expire meds
                var thresholdDate = today.AddDays(30);
                //search all meds that are soon to expire and include user model in the result
                var expiringMed = await _context.Medicines
                    .AsNoTracking()
                    .Include(m => m.User)
                    .Where(m => m.ExpiryDate >= today && m.ExpiryDate <= thresholdDate && m.User.CheckRequired == true)
                    .ToListAsync();
                //group the results by user
                var expiringMedByUser = expiringMed
                    .GroupBy(m => m.User)
                    .ToList();
                //send messages to each user that has soon to expire meds
                foreach (var group in expiringMedByUser)
                {
                    var user = group.Key;
                    var userMed = group.ToList();

                    var message = "**Срок годности истекает:**\n" + string.Join("\n", userMed.Select(m => $"- {m.Name} ({m.ExpiryDate:dd.MM.yyyy})"));
                    try
                    {
                        await _botClient.SendMessage(
                            chatId: (long)user.TgChatId,
                            text: message,
                            parseMode: ParseMode.Markdown);
                        _logger.LogInformation($"Уведомление отправлено пользователю {user.TgId}");

                    }
                    //if the user had blocked the bot, delete the user's records from the bot
                    catch (ApiRequestException ex) when (ex.ErrorCode == 403)
                    {
                        _logger.LogWarning(ex, $"Пользователь {user.TgId} заблокировал бота.");
                        //Chat and User IDs are being stored in BigInteger format in the DB
                        await _dbMethods.DeleteUser((long)user.TgId);
                    }

                }
            }
        }
    }
}
