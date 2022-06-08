﻿using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using KHAIScheduleBot.Services;
using KHAIScheduleBot;
using Telegram.Bot.Types;
using KHAIScheduleBot.Controllers;

IFileConfig _fileConfig = new FileConfig();
TelegramBotClient bot = new TelegramBotClient(_fileConfig.FileToken);

User me = await bot.GetMeAsync();
Console.Title = me.Username ?? "My awesome Bot";

using var cts = new CancellationTokenSource();

BotController handlersService = new BotController(new ParserServices(_fileConfig));

//set commands for bot
string[] commands = new string[] { "/help", "/group_set", "/week_set", "/group_get", "/week_get", "/schedule_day_get", "/schedule_week_get", "/keyboard" };
string[] descriptions = new string[] { "показати команди","встановити групу", "встановити тип тижня", "показати групу", "показати тип тижня", "показати розклад на день", "показати розклад на тиждень", "показати клавіатуру" };
BotCommand[] botCommands = new BotCommand[commands.Length];
for (int i = 0; i < commands.Length; i++)
{
    botCommands[i] = new BotCommand() { Command = commands[i], Description = descriptions[i] };
}
await bot.SetMyCommandsAsync(botCommands);

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
bot.StartReceiving(updateHandler: handlersService.HandleUpdateAsync,
                   errorHandler: handlersService.HandleErrorAsync,
                   receiverOptions: new ReceiverOptions()
                   {
                       AllowedUpdates = Array.Empty<UpdateType>()
                   },
                   cancellationToken: cts.Token);

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();