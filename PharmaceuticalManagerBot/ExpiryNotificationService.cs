using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using PharmaceuticalManagerBot.Data;
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
    internal class ExpiryNotificationService : BackgroundService
    {
        private readonly ILogger<ExpiryNotificationService> _logger;
        private readonly PharmaceuticalManagerBotContext _context;
        private readonly ITelegramBotClient _botClient;

        public ExpiryNotificationService(
            ILogger<ExpiryNotificationService> logger,
            PharmaceuticalManagerBotContext context,
            ITelegramBotClient botClient)
        {
            _logger = logger;
            _context = context;
            _botClient = botClient;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckAndNotifyAsync();
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CheckAndNotifyAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var thresholdDate = today.AddDays(30);
            var expiringMed = await _context.Medicines
                .Include(m => m.User)
                .Where(m => m.ExpiryDate >= today && m.ExpiryDate <= thresholdDate && m.User.CheckRequired == true)
                .ToListAsync();
            var expiringMedByUser = expiringMed
                .GroupBy(m => m.User)
                .ToList();

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
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    _logger.LogInformation($"Уведомление отправлено пользователю {user.TgId}");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка оправки уведомления для {user.TgId}");
                }

            }
        }
    }
}
