using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using KHAIScheduleBot.Services;
using KHAIScheduleBot.Extansions;
using KHAIScheduleBot.Models;

namespace KHAIScheduleBot.Controllers
{

    public class BotController
    {
        #region Dependency injection
        // seervice for parsing the data.
        private readonly IParserService _parserService;
        // Client of the telegram bot.
        private ITelegramBotClient _botClient;
        // service for parsing configuration from config file.
        private readonly IBotConfig _botConfig;
        #endregion
        #region Fields
        // group id
        private string groupId;
        // type of week.
        private WeekType typeOfWeek;
        // talk that now setting group id.
        private bool isGroup;
        // talk that now setting day.
        private bool isDay;
        // talk that now setting type of week id.
        private bool isTypeOfWeek;
        // List with all groups id.
        private List<string> groupsID;
        #endregion
        /// <summary>
        /// Contructor for initialize BotController.
        /// </summary>
        /// <param name="parserService">Service for parsing the data.</param>
        /// <param name="botConfig">Service for reading config information from config file.</param>
        public BotController(IParserService parserService, IBotConfig botConfig)
        {
            _parserService = parserService;
            _botConfig = botConfig;
            typeOfWeek = WeekType.Both;
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
            if (text.StartsWith("/help"))
                messageText = "/help";

            Task <Message> action = messageText switch
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
        }

        // Process Inline Keyboard callback data
        async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            string data = callbackQuery.Data;

            if (data.GetDayType()!=DayType.None)
                data = "day";
            else if (data.GetWeekType()!=WeekType.None)
                data = "week";
            else if (groupsID != null && groupsID.Contains(data))
                data = "group";

            switch (data)
            {
                case "menu": await SendMainKeyboard(callbackQuery.Message); break;
                case "commands": await SendCommands(callbackQuery.Message); break;
                case "week": 
                    isTypeOfWeek = true; 
                    await ProcessWeek(new Message() { Chat = callbackQuery.Message.Chat, Text = callbackQuery.Data }); 
                    break;
                case "day": 
                    isDay = true; 
                    await ProcessSchedule(new Message() { Chat = callbackQuery.Message.Chat, Text = callbackQuery.Data }); 
                    break;
                case "group":
                    isGroup = true;
                    await ProcessGroup(new Message() { Chat = callbackQuery.Message.Chat, Text = callbackQuery.Data });
                    break;
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
            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: "Головну клавіатру додано. Можете користуватися.",
                                                        replyMarkup: replyKeyboardMarkup);
        }        

        // Send reply group keyboard
        async Task<Message> SendGroupKeyboard(Message message)
        {
            IReplyMarkup replyMarkup = new ReplyKeyboardMarkup(
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
                                        replyMarkup: replyMarkup);
        }

        // Send reply week keyboard
        async Task<Message> SendWeekKeyboard(Message message)
        {
            IReplyMarkup replyMarkup = new ReplyKeyboardMarkup(
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
                                        replyMarkup: replyMarkup);
        }

        // Send bot commands
        async Task<Message> SendCommands(Message message)
        {
            string commands = "💻Команди:💻\n" +
                                 "/group_set      -> встановити групу\n" +
                                 "/week_set       -> встановити тип тижня\n" +
                                 "/group_get      -> показати групу\n" +
                                 "/week_get       -> показати тип тижня\n" +
                                 "/schedule_day_get   -> показати розклад на день\n" +
                                 "/schedule_week_get  -> показати розклад на тиждень\n" +
                                 "/help           -> показати команди\n" +
                                 "/keyboard       -> показати клавіатуру\n";

            if (message.Chat.Type != ChatType.Private)
                commands = "💻Команди:💻\n" +
                                 $"/group_set{this._botConfig.BotName} -> встановити групу\n" +
                                 $"/week_set{this._botConfig.BotName}  -> встановити тип тижня\n" +
                                 $"/group_get{this._botConfig.BotName} -> показати групу\n" +
                                 $"/week_get{this._botConfig.BotName} -> показати тип тижня\n" +
                                 $"/schedule_day_get{this._botConfig.BotName}  -> показати розклад на день\n" +
                                 $"/schedule_week_get{this._botConfig.BotName} -> показати розклад на тиждень\n" +
                                 $"/help{this._botConfig.BotName}  -> показати команди\n";

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: commands);
        }

        // Send a help message
        async Task<Message> SendHelpMessage(Message message)
        {
            string help = "❗️Такої команди не усніє❗️ скористайтеся командою /help або введіть / для переглянуд усіх" +
                "достуних команд.";
            if (message.Chat.Type == ChatType.Private)
                help = $"❗️Такої команди не усніє❗️ скористайтеся командою /help{this._botConfig.BotName} або введіть / для переглянуд усіх" +
                "достуних команд. Також можете скористуватися клавіатурою.";

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: help);
        }

        // Process set or get group commands
        async Task<Message> ProcessGroup(Message message)
        {
            string textMessage = default;
            string[] commands = message.Text.Split('_');

            //send user message where the bot asks the user to select group
            if (message.Text == "🖌Встановити групу" || (commands.Length > 1 && commands[1].Contains("set")))
            {
                if(this.groupsID==null)
                    this.groupsID = _parserService.GetAllGroupsId();
                textMessage = "👥Оберіть групу👥";
                IReplyMarkup inlineReplyMarkup = FillGroupIdInlineKeyboard();
                isGroup = true;
                return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: textMessage,
                                                        replyMarkup: inlineReplyMarkup);
            }

            //set group
            if (this.isGroup)
            {
                if (this.groupsID.IndexOf(message.Text)>=0)
                {
                    textMessage = "Групу встановлено.";
                    this.groupId = message.Text;
                    isGroup = !isGroup;
                }
                else
                    textMessage = "Такої групи не існує. Введіть знову.";
            }
            //send group to the user
            else
            {
                if (string.IsNullOrEmpty(this.groupId))
                    textMessage = "Групу не встановленно.";
                else
                    textMessage = $"Група: {this.groupId}";
            }

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: textMessage);
        }

        // Process set or get week commands
        async Task<Message> ProcessWeek(Message message)
        {
            string textMessage = default;
            string[] commands = message.Text.Split('_');
            
            //send user message with inline keyboard for select typeofweek
            if (message.Text == "🖌Встановити тиждень" || (commands.Length > 1 && commands[1].Contains("set")))
            {
                textMessage = "🗂Оберіть тип тижня🗂";
                IReplyMarkup replyMarkup = new InlineKeyboardMarkup(
                    new[]
                    {
                        // first row
                        new []{ InlineKeyboardButton.WithCallbackData(WeekType.Numerator.GetString()) },
                        // second row
                        new [] { InlineKeyboardButton.WithCallbackData(WeekType.Denomanator.GetString()) },
                        // third row
                        new [] { InlineKeyboardButton.WithCallbackData(WeekType.Both.GetString()) }
                    });
                isTypeOfWeek = true;
                return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                            text: textMessage,
                                            replyMarkup: replyMarkup);
            }
            //set typeofweek
            if (this.isTypeOfWeek)
            {
                this.typeOfWeek = message.Text.GetWeekType();
                if (typeOfWeek != WeekType.None)
                {
                    textMessage = "Тип тижню встановлено.";
                    isTypeOfWeek = !isTypeOfWeek;
                }
                else
                    textMessage = "Некоректний тип тижню❗️Оберіть тип знову.";
            }
            //send type of week to the user 
            else
            {
                if (this.typeOfWeek== WeekType.None)
                    textMessage = "Тип тижня не встановленно.";
                else
                    textMessage = $"Тип тижня: {this.typeOfWeek.GetString()}";
            }

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: textMessage);
        }

        // proccess get schedule commands 
        async Task<Message> ProcessSchedule(Message message)
        {
            string textMessage = default;
            ParseMode? mode = null;
            string[] commands = message.Text.Split('_');
            if (!string.IsNullOrEmpty(this.groupId))
            {
                //ask to select day
                if (commands[0] == "📅Розклад на день" || (commands.Length > 1 && commands[1] == "day"))
                {
                    textMessage = "📅Оберіть день📅";
                    IReplyMarkup replyMarkup = new InlineKeyboardMarkup(
                            new[]
                            {
                            // first row
                            new []{ InlineKeyboardButton.WithCallbackData(DayType.Monday.GetString()) },
                            // second row
                            new [] { InlineKeyboardButton.WithCallbackData(DayType.Tuesday.GetString()) },
                            // third row
                            new [] { InlineKeyboardButton.WithCallbackData(DayType.Wednesday.GetString()) },
                            // fourth row
                            new [] { InlineKeyboardButton.WithCallbackData(DayType.Thursday.GetString()) },
                            // fifth row
                            new [] { InlineKeyboardButton.WithCallbackData(DayType.Friday.GetString()) }
                            });
                    isDay = true;
                    return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                     text: textMessage,
                                     replyMarkup: replyMarkup);

                }
                //get schedule for some day
                if (this.isDay)
                {
                    DayType day = message.Text.GetDayType();
                    if (day!=DayType.None)
                    {
                        Group group = _parserService.GetSchedule(this.groupId, day, typeOfWeek);
                        textMessage = group.ToString();
                        isDay = !isDay;
                        mode = ParseMode.Markdown;
                    }
                    else
                        textMessage = "Некоректні дані❗️Оберіть день знову.";
                }
                //get schedule for whole week
                else
                {
                    Group group = _parserService.GetSchedule(this.groupId, DayType.None, typeOfWeek);
                    textMessage = group.ToString();
                    mode = ParseMode.Markdown;
                }
            }
            else
                textMessage = "Спочатку необхідно встановити групу.";
            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                             text: textMessage,
                                             parseMode: mode);
        }

        //check command for correctness.
        /* command - command from user */
        void CheckCommands(ref string command)
        {
            string[] commands = new string[] { "/start", "/keyboard", "/help", "/group_set", "/week_set", "/group_get", "/week_get", "/schedule_week_get",
                "/schedule_day_get", $"/help{this._botConfig.BotName}", $"/group_set{this._botConfig.BotName}", 
                $"/week_set{this._botConfig.BotName}", $"/group_get{this._botConfig.BotName}", $"/week_get{this._botConfig.BotName}", $"/schedule_week_get{this._botConfig.BotName}",
                $"/schedule_day_get{this._botConfig.BotName}"};

            string[] keyboards = new string[] {"🔎Показати тиждень", "🖌Встановити тиждень", "🔙Головне меню","🖌Встановити групу", "🔎Показати групу","💻Команди",
                "👥Група","📅Розклад на день", "🗓Розклад на тиждень", "🗂Тиждень"};
            
            bool isCorrect = commands.Contains(command) || keyboards.Contains(command);
            
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

        //create inline keyboard with the groups id.
        InlineKeyboardMarkup FillGroupIdInlineKeyboard()
        {
            int countGroup = this.groupsID.Count;
            int numColumns = 3;
            int numRows = (int)Math.Ceiling((decimal)countGroup / 3);
            InlineKeyboardButton[][] inlineKeyboardButtons = new InlineKeyboardButton[numRows][];
            
            for(int i = 0; i<numRows; i++)
            {
                int numElements = numColumns;
                if(i == numRows - 1)
                    numElements = countGroup - i * numColumns;

                inlineKeyboardButtons[i] = new InlineKeyboardButton[numElements];

                for (int j = 0; j < numElements; j++)
                    inlineKeyboardButtons[i][j] = InlineKeyboardButton.WithCallbackData(this.groupsID[(i * numColumns + j)]);
            }
            
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
            
        }
    }
}