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

            string text = message.Text;

            //check command for correctness.
            CheckCommands(ref text);

            string messageText = text.Split('_')[0];

            if (text == "🖌Встановити групу" || text == "🔎Показати групу" || isGroup)
                messageText = "/group";
            if (text == "🖌Встановити тиждень" || text == "🔎Показати тиждень" || isTypeOfWeek)
                messageText = "/week";
            if (isDay)
                messageText = "/schedule";

            Task<Message> action = messageText switch
            {
                "/start" => SendInlineKeyboard(message),
                "/keyboard" => SendMainKeyboard(message),
                "👥Група" => SendGroupKeyboard(message),
                "/group" => ProcessGroup(message),
                "🗂Тиждень" => SendWeekKeyboard(message),
                "/week" => ProcessWeek(message),
                "📅Розклад на день" or "🗓Розклад на тиждень" or "/schedule" => ProcessSchedule(message),
                "🔙Головне меню" => SendMainKeyboard(message),
                "💻Команди" or "/help" => SendCommands(message),
                _ => SendHelpMessage(message)

            };
            Message sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");
        }

        // Process Inline Keyboard callback data
        async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            switch (callbackQuery.Data)
            {
                case "menu": await SendMainKeyboard(callbackQuery.Message); break;
                case "commands": await SendCommands(callbackQuery.Message); break;
                case "Чисельник":
                case "Знаменник":
                case "Обидва": isTypeOfWeek = true; await ProcessWeek(new Message() { Chat = callbackQuery.Message.Chat, Text = callbackQuery.Data }); break;
                case "Понеділок":
                case "Вівторок":
                case "Середа":
                case "Четвер":
                case "П'ятниця": isDay = true; await ProcessSchedule(new Message() { Chat = callbackQuery.Message.Chat, Text = callbackQuery.Data }); break;
            }
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

        // Send reply main keyboard
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
        
        // Send reply group keyboard
        async Task<Message> SendGroupKeyboard(Message message)
        {
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

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                        text: "Додаткову клавіатуру додано.Можете користуватися",
                                        replyMarkup: this.botKeyboard);
        }
        // Send reply week keyboard
        async Task<Message> SendWeekKeyboard(Message message)
        {
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

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                        text: "Додаткову клавіатуру додано.Можете користуватися",
                                        replyMarkup: this.botKeyboard);
        }

        // Send bot commands
        async Task<Message> SendCommands(Message message)
        {
            const string commands = "💻Команди:💻\n" +
                                 "/group_set      -> встановити групу\n" +
                                 "/week_set       -> встановити тип тижня\n" +
                                 "/group_get      -> показати групу\n" +
                                 "/week_get       -> показати тип тижня\n" +
                                 "/schedule_day_get   -> показати розклад на день\n" +
                                 "/schedule_week_get  -> показати розклад на тиждень\n" +
                                 "/help           -> показати команди\n" +
                                 "/keyboard       -> показати клавіатуру\n";

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: commands);
        }
        // Send a help message
        async Task<Message> SendHelpMessage(Message message)
        {
            const string help = "❗️Такої команди не усніє❗️ скористайтеся командою /help або введіть / для переглянуд усіх" +
                "достуних команд. Також можете скористуватися клавіатурою.";

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: help);
        }
        // Process set or get group commands
        async Task<Message> ProcessGroup(Message message)
        {
            string textMessage = default;
            string[] commands = message.Text.Split('_');

            //set group
            if (this.isGroup)
            {
                if (_parserService.GroupExist(message.Text))
                {
                    textMessage = "Групу встановлено.";
                    this.group = message.Text;
                    isGroup = !isGroup;
                }
                else
                    textMessage = "Такої групи не існує. Введіть знову.";
            }
            //send user message where the bot asks the user to send group
            else if (message.Text == "🖌Встановити групу" || (commands.Length > 1 && commands[1] == "set"))
            {
                textMessage = "Відправьте номер групи. Букву групи вказувати українською/російською.";
                isGroup = true;
            }
            //send group to the user
            else
            {
                if (string.IsNullOrEmpty(this.group))
                    textMessage = "Групу не встановленно.";
                else
                    textMessage = $"Група: {this.group}";
            }

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: textMessage);
        }
        // Process set or get week commands
        async Task<Message> ProcessWeek(Message message)
        {
            string textMessage = default;
            string[] types = new string[] { "Чисельник", "Знаменник", "Обидва" };
            string[] commands = message.Text.Split('_');
            
            //send user message with inline keyboard for select typeofweek
            if (message.Text == "🖌Встановити тиждень" || (commands.Length > 1 && commands[1] == "set"))
            {
                textMessage = "🗂Оберіть тип тижня🗂";
                IReplyMarkup replyMarkup = new InlineKeyboardMarkup(
                    new[]
                    {
                        // first row
                        new []{ InlineKeyboardButton.WithCallbackData(types[0]) },
                        // second row
                        new [] { InlineKeyboardButton.WithCallbackData(types[1]) },
                        // third row
                        new [] { InlineKeyboardButton.WithCallbackData(types[2]) }
                    });
                isTypeOfWeek = true;
                return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                            text: textMessage,
                                            replyMarkup: replyMarkup);
            }
            //set typeofweek
            if (this.isTypeOfWeek)
            {
                if (types.Contains(message.Text))
                {
                    textMessage = "Тип тижню встановлено.";
                    this.typeOfWeek = message.Text;
                    isTypeOfWeek = !isTypeOfWeek;
                }
                else
                    textMessage = "Некоректний тип тижню❗️Оберіть тип знову.";
            }
            //send type of week to the user 
            else
            {
                if (string.IsNullOrEmpty(this.typeOfWeek))
                    textMessage = "Тип тижня не встановленно.";
                else
                    textMessage = $"Тип тижня: {this.typeOfWeek}";
            }

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: textMessage);
        }
        // proccess get schedule commands 
        async Task<Message> ProcessSchedule(Message message)
        {
            string textMessage = default;
            string[] days = new string[] { "Понеділок", "Вівторок", "Середа", "Четвер", "П'ятниця" };
            string[] commands = message.Text.Split('_');
            
            //ask to select day
            if (commands[0] == "📅Розклад на день" || commands[1] == "day")
            {
                textMessage = "📅Оберіть день📅";
                IReplyMarkup replyMarkup = new InlineKeyboardMarkup(
                        new[]
                        {
                            // first row
                            new []{ InlineKeyboardButton.WithCallbackData(days[0]) },
                            // second row
                            new [] { InlineKeyboardButton.WithCallbackData(days[1]) },
                            // third row
                            new [] { InlineKeyboardButton.WithCallbackData(days[2]) },
                            // fourth row
                            new [] { InlineKeyboardButton.WithCallbackData(days[3]) },
                            // fifth row
                            new [] { InlineKeyboardButton.WithCallbackData(days[4]) }
                        });
                isDay = true;
                return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                 text: textMessage,
                                 replyMarkup: replyMarkup);

            }
            //get schedule for some day
            if (this.isDay)
            {
                if (days.Contains(message.Text))
                {
                    isDay = !isDay;
                }
                else
                    textMessage = "Некоректні дані❗️Оберіть день знову.";
            }

            //get schedule for whole week
            else
            {

            }

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                             text: textMessage);
        }
        //check command for correctness.
        /* command - command from user */
        void CheckCommands(ref string command)
        {
            string[] commands = new string[] { "/start", "/keyboard", "/help", "/group_set", "/week_set", "/group_get", "/week_get", "/schedule_week_get",
                "/schedule_day_get", "🔎Показати тиждень", "🖌Встановити тиждень", "🔙Головне меню","🖌Встановити групу", "🔎Показати групу","💻Команди",
                "👥Група","📅Розклад на день", "🗓Розклад на тиждень", "🗂Тиждень"};
            
            bool isCorrect = commands.Contains(command);
            
            if (isGroup || isDay || isTypeOfWeek)
            {
                if (isCorrect)
                {
                    isGroup = false;
                    isDay = false;
                    isTypeOfWeek = false;
                }
            }
            else
            {
                if (!isCorrect)
                    command = "uncorrect";
            }


        }
    }
}