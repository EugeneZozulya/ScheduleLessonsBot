using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Threading;
using System.IO;
using System.Linq;
using KHAIScheduleBot.Services;

namespace KHAIScheduleBot.Controllers
{

    public class BotController
    {
        #region Dependency injection
        private readonly IParserService _parserService;
        private ITelegramBotClient _botClient;
        #endregion
        #region Fields
        private string group;
        private string day;
        private string typeOfWeek;
        private bool isGroup;
        private bool isDay;
        private bool isTypeOfWeek;
        private ReplyKeyboardMarkup botKeyboard;
        #endregion
        public BotController(IParserService parserService)
        {
            _parserService = parserService;
        }
        /// <summary>
        /// Procces handle error.
        /// </summary>
        /// <param name="botClient">Telegram bot clien.</param>
        /// <param name="exception">Thrown Exception.</param>
        /// <param name="cancellationToken"> Cancellation token for cansel thread. </param>
        /// <returns></returns>
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        /// <summary>
        /// Procces handle update.
        /// </summary>
        /// <param name="botClient">Telegram bot clien.</param>
        /// <param name="update">Update.</param>
        /// <param name="cancellationToken"> Cancellation token for cansel thread. </param>
        /// <returns></returns>
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            _botClient = botClient;
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(_botClient, exception, cancellationToken);
            }
        }
        // Process Message received.
        async Task BotOnMessageReceived(Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            Task<Message> action = null;

            //check input message when user sent value set day,group and typeofweek.
            CheckReservedWords(message.Text);

            string messageText = message.Text!.Split('_')[0];
            if (message.Text == "🖌Встановити групу" || message.Text == "🔎Показати групу" || isGroup)
                messageText = "/group";
            if (message.Text == "🖌Встановити тиждень" || message.Text == "🔎Показати тиждень" || isTypeOfWeek)
                messageText = "/week";
            switch (messageText)
            {
                case "/start": action = SendInlineKeyboard(message); break;
                case "💻Команди":
                case "/help": action = SendCommands(message); break;
                case "👥Група":
                case "/group": action = ProcessGroup(message); break;
                case "📅Розклад на день":
                case "/day": action = ProcessDay(message); break;
                case "🗂Тиждень":
                case "/week": action = ProcessWeek(message); break;
                case "schedule": action = ProcessSchedule(message); break;
                case "🔙Головне меню": action = SendMainKeyboard(message); break;
                default: break;

            };
            Message sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");
        }

        // Process Inline Keyboard callback data
        async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            var action = callbackQuery.Data switch
            {
                "menu" => SendMainKeyboard(callbackQuery.Message),
                "commands" => SendCommands(callbackQuery.Message)
            };

            await action;
        }

        // Process unknown update handler.
        Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        // Send inline keyboard
        // Process responses in BotOnCallbackQueryReceived handler
        async Task<Message> SendInlineKeyboard(Message message)
        {
            await _botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            await Task.Delay(500);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                        // first row
                        new []{ InlineKeyboardButton.WithCallbackData("💻Команди", "commands") },
                        // second row
                        new [] { InlineKeyboardButton.WithCallbackData("⌨️Клавітура", "menu") }
                });

            string text = "📃Мое завдання📃: надсилати розклад пар на весь тиждень, при цьому можна вказати тип тижня (числитель/знаменник/обидва) або на певний день. " +
                        "Для початку роботи необхідно обов'язково вказати групу, для якої показувати розклади. Взаємодіяти зі мною можна використовуючи клавітуру⌨️ або відправляти команди💻.";

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: text,
                                                        parseMode: ParseMode.Html,
                                                        replyMarkup: inlineKeyboard);
        }

        // Send reply keyboard
        // Process responses in BotOnCallbackQueryReceived handler
        async Task<Message> SendMainKeyboard(Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[] {
                    new KeyboardButton[] { "💻Команди" },
                    new KeyboardButton[] { "👥Група" },
                    new KeyboardButton[] { "🗂Тиждень" },
                    new KeyboardButton[] { "📅Розклад на день" },
                    new KeyboardButton[] { "🗓Розклад на тиждень" }
                })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
            this.botKeyboard = replyKeyboardMarkup;
            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: "Головну клавіатру додано. Можете користуватися.",
                                                        replyMarkup: replyKeyboardMarkup);
        }

        // Send bot commands
        async Task<Message> SendCommands(Message message)
        {
            const string commands = "💻Команди:💻\n" +
                                 "/group_set      -> встановити групу\n" +
                                 "/week_set       -> встановити тип тижня\n" +
                                 "/day_set        -> встановити день\n" +
                                 "/group_get      -> показати групу\n" +
                                 "/week_get       -> показати тип тижня\n" +
                                 "/day_get        -> показати день для пошуку\n" +
                                 "/schedule_get   -> показати розклад\n" +
                                 "/help           -> показати команди\n";

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: commands,
                                                        replyMarkup: this.botKeyboard);
        }

        async Task<Message> ProcessGroup(Message message)
        {
            string textMessage = default;
            string[] commands = message.Text.Split('_');
            if ((commands[0] == "👥Група" && commands.Length > 1) || commands.Length > 2)
                textMessage = "Некоректно введено команду❗️";
            {
                if (this.isGroup)
                {
                    textMessage = "Групу встановлено.";
                    this.group = message.Text;
                    isGroup = !isGroup;
                }
                else if (commands[0] == "👥Група")
                {
                    textMessage = "Додаткову клавіатуру додано.Можете користуватися";
                    botKeyboard = new ReplyKeyboardMarkup(
                        new[] {
                            new KeyboardButton[] { "🔎Показати групу" },
                            new KeyboardButton[] { "🖌Встановити групу" },
                            new KeyboardButton[] { "🔙Головне меню" },
                        })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };
                }
                else if (message.Text == "🖌Встановити групу" || (commands.Length > 1 && commands[1] == "set"))
                {
                    textMessage = "Відправьте номер групи. Букву групи вказувати українською/російською.";
                    isGroup = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(this.group))
                        textMessage = "Групу не встановленно.";
                    else
                        textMessage = $"Група: {this.group}";
                }
            }

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: textMessage,
                                                        replyMarkup: this.botKeyboard);
        }
        async Task<Message> ProcessDay(Message message)
        {
            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                            text: "cum");
        }
        async Task<Message> ProcessWeek(Message message)
        {
            string textMessage = default;
            string[] commands = message.Text.Split('_');
            if ((commands[0] == "🗂Тиждень" && commands.Length > 1) || commands.Length > 2)
                textMessage = "Некоректно введено команду❗️";
            {
                if (this.isTypeOfWeek)
                {
                    textMessage = "Тиждень встановлено.";
                    this.typeOfWeek = message.Text;
                    isTypeOfWeek = !isTypeOfWeek;
                }
                else if (commands[0] == "🗂Тиждень")
                {
                    textMessage = "Додаткову клавіатуру додано.Можете користуватися";
                    botKeyboard = new ReplyKeyboardMarkup(
                        new[] {
                            new KeyboardButton[] { "🔎Показати тиждень" },
                            new KeyboardButton[] { "🖌Встановити тиждень" },
                            new KeyboardButton[] { "🔙Головне меню" },
                        })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };
                }
                else if (message.Text == "🖌Встановити тиждень" || (commands.Length > 1 && commands[1] == "set"))
                {
                    textMessage = "Відправьте тип тижня. Допускаються такі варіанти: Знаменник, Чисельник, Обидва";
                    isTypeOfWeek = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(this.typeOfWeek))
                        textMessage = "Тип тижня не встановленно.";
                    else
                        textMessage = $"Тип тижня: {this.typeOfWeek}";
                }
            }

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: textMessage,
                                                        replyMarkup: this.botKeyboard);
        }
        async Task<Message> ProcessSchedule(Message message)
        {
            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                            text: "cum");
        }

        void CheckReservedWords(string inputData)
        {
            if (isGroup || isDay || isTypeOfWeek)
            {
                string[] reserved = new string[] { "/help", "/group_set", "/week_set", "/day_set", "/group_get", "/week_get", "/day_get",
                "/schedule_get", "🔎Показати тиждень", "🖌Встановити тиждень", "🔙Головне меню","🖌Встановити групу", "🔎Показати групу","💻Команди",
                "👥Група","📅Розклад на день", "🗓Розклад на тиждень"};
                if (reserved.Contains(inputData))
                {
                    isGroup = false;
                    isDay = false;
                    isTypeOfWeek = false;
                }
            }

        }
    }
}