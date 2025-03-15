using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PharmaceuticalManagerBot.Data;
using PharmaceuticalManagerBot.Methods;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace PharmaceuticalManagerBot
{
    public class PharmaceuticalManagerBotWorker : BackgroundService
    {
        private readonly ILogger<PharmaceuticalManagerBotWorker> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly IUserStateTracker _userState;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CancellationTokenSource _cts = new();

        public PharmaceuticalManagerBotWorker(ILogger<PharmaceuticalManagerBotWorker> logger, ITelegramBotClient botClient, IUserStateTracker userState, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _botClient = botClient;
            _userState = userState;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Запуск бота...");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery,
                },
                DropPendingUpdates = true
            };

            _botClient.StartReceiving
                (
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: _cts.Token
                );
            _logger.LogInformation("Бот запущен. Ожидание сообщений...");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Остановка бота...");
            _cts.Cancel();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //Track a state of the chat
            var userState = _userState;
            //Methods to interract with DB
            using (var scope = _scopeFactory.CreateScope())
            {
                DbMethods dbMethods = scope.ServiceProvider.GetRequiredService<DbMethods>();

                //Interract with user's messages
                if (update.Type == UpdateType.Message)
                {
                    var msg = update.Message;
                    string? state = userState.GetState(msg.Chat.Id);
                    try
                    {
                        if (state == null)
                        {
                            _logger.LogInformation($"Получено сообщение от {msg.From?.Username}: {msg.Text}");

                            switch (msg.Text)
                            {
                                //Greets and adds user to the DB, 
                                case "/start":
                                    bool userAdded = await dbMethods.AddUser(msg.From.Id, msg.Chat.Id);
                                    if (userAdded)
                                    {
                                        await botClient.SendMessage(
                                        chatId: msg.Chat,
                                        text: $"Привет {msg.Chat.FirstName}! Я аптечный менеджер и помогу тебе следить за сроком годности препаратов. Для полного списка команд отправь /help. Советую ам начать с команды /show_types, чтобы получить список с порядковыми номерами типов лекарственных средств и закрепить этот список.");
                                    }
                                    break;
                                //Sends an instruction to a user on how to write a request to add a emd to the DB + sets a state for user to track the next message
                                case "/add":
                                    await botClient.SendMessage(
                                        chatId: msg.Chat,
                                        text: "Чтобы добавить препарат, отправь данные в следующем формате: Название|Активное вещество|Тип препарата (номер из списка)|Срок годности (дд-мм-гггг)\nПример: Ибуметин 400мг|Ибупрофен|14|28-10-2028");
                                    _userState.SetState(msg.Chat.Id, "GET_MED_DETAILS");
                                    break;
                                //Shows a list of all user's meds
                                case "/show":
                                    await dbMethods.GetMedList(botClient, msg.From.Id, msg.Chat.Id);
                                    break;
                                //Shows a list of types of medications with their IDs
                                case "/show_types":
                                    await _botClient.SendMessage(
                                        chatId: msg.Chat,
                                        text: dbMethods.GetMedTypes());
                                    break;
                                //Shows a list of meds that are soon to expire
                                case "/expire_soon":
                                    await dbMethods.GetMedicineSoonExpireOneUser(botClient, msg.Chat.Id, msg.From.Id);
                                    break;
                                //Shows a list of meds that are expired
                                case "/show_expired":
                                    await dbMethods.GetExpiredMed(botClient, msg.Chat.Id, msg.From.Id);
                                    break;
                                //Enable automatic check for the meds that are soon to expire
                                case "/auto_check":
                                    try
                                    {
                                        await dbMethods.EnableAutoCheck(msg.From.Id);
                                        await botClient.SendMessage(
                                        chatId: msg.Chat,
                                        text: "Автоматическая проверка и рассылка включена!");
                                    }
                                    catch (Exception ex)
                                    {
                                        await botClient.SendMessage(
                                            chatId: msg.Chat,
                                            text: ex.Message);
                                    }

                                    break;
                                //Disable automatic check for the meds that are soon to expire
                                case "/auto_check_disable":
                                    try
                                    {
                                        await dbMethods.DisableAutoCheck(msg.From.Id);
                                        await botClient.SendMessage(
                                        chatId: msg.Chat,
                                        text: "Автоматическая проверка и рассылка включена!");
                                    }
                                    catch (Exception ex)
                                    {
                                        await botClient.SendMessage(
                                            chatId: msg.Chat,
                                            text: ex.Message);
                                    }
                                    break;
                                //This is a help command that describes what other commands do
                                case "/help":
                                    await botClient.SendMessage(
                                        chatId: msg.Chat,
                                        text: "Список команд:\n/add — добавить препарат\n/show — отобразить список добавленных препаратов\n/show_types — отобразить полный список типов препаратов\n/expire_soon — отобразить список препаратов, срок годности которых закончится через месяц\n/show_expired — показать препараты, срок годности которых подошёл к концу\n/auto_check — включить автоматическую проверку и рассылку списка препаратов, срок годности которых закончится через месяц\n/auto_check_disable — выключить автоматическую проверку и рассылку");
                                    break;
                                default:
                                    await botClient.SendMessage(
                                        chatId: msg.Chat,
                                        text: "Для взаимодействия с ботом, пожалуйста, используйте только указанные команды.");
                                    break;
                            }
                        }
                        else if (userState.GetState(msg.Chat.Id) != null)
                        {
                            //string state = userState.GetState(msg.Chat.Id);
                            switch (state)
                            {
                                //Message under this state is expected to have a string with med details
                                case "GET_MED_DETAILS":
                                    try
                                    {
                                        //Cancellation 
                                        if (msg.Text.ToLowerInvariant().Equals("отмена"))
                                        {
                                            userState.RemoveState(msg.Chat.Id);
                                            break;
                                        }

                                        await dbMethods.AddMed(msg.From.Id, msg.Text);
                                        await botClient.SendMessage(
                                            chatId: msg.Chat,
                                            text: "Поздравляю! Препарат был успешно добавлен!");
                                        _logger.LogInformation($"Препарат успешно добавлен для пользователя {msg.From.Id}");
                                        userState.RemoveState(msg.Chat.Id);
                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        await botClient.SendMessage(
                                                chatId: msg.Chat,
                                                text: ex.Message + "\nЕсли вы передумали добавлять препарат, то напишите \"отмена\"");
                                        _logger.LogError($"Ошибка при добавлении препарата для {msg.From.Id}.\nОшибка: {ex.Message}");
                                        break;
                                    }
                                default:
                                    await botClient.SendMessage(
                                        chatId: msg.Chat,
                                        text: "Произошла ошибка. Пожалуйста, повторите запрос.");
                                    _logger.LogError($"Произошла ошибка с со статусом у пользователя {msg.From.Id}");
                                    break;
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка обработки сообщения");
                    }
                }
                //Interract with user's nav buttons
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    CallbackQuery callback = update.CallbackQuery;
                    string[] data = callback.Data.Split('_');
                    long chatId = callback.Message.Chat.Id;
                    int msgId = callback.Message.MessageId;
                    long userId = callback.From.Id;

                    switch (data[0])
                    {
                        case "med":
                            int medId = int.Parse(data[1]);
                            try
                            {
                                await dbMethods.GetSingleMed(botClient, chatId, medId);
                                break;
                            }
                            catch (Exception ex)
                            {
                                await botClient.SendMessage(
                                    chatId: chatId,
                                    text: ex.Message);
                                break;
                            }
                        case "page":
                            await dbMethods.GetMedList(botClient, userId, chatId, int.Parse(data[1]));
                            await botClient.DeleteMessage(chatId, msgId);
                            break;
                        case "close":
                            await botClient.DeleteMessage(chatId, msgId);
                            break;
                        case "delete":
                            try
                            {
                                await dbMethods.DeleteSingleMed(botClient, chatId, int.Parse(data[1]));
                            }
                            catch (Exception ex)
                            {
                                await botClient.SendMessage(
                                    chatId: chatId,
                                    text: ex.Message);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Ошибка Telegram API: {apiRequestException.ErrorCode} - {apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(errorMessage);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _cts.Cancel();
            base.Dispose();
        }
    }
}
