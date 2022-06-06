using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using KHAIScheduleBot.Services;
using KHAIScheduleBot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

IFileConfig _fileConfig = new FileConfig();
TelegramBotClient bot = new TelegramBotClient(_fileConfig.FileToken);

User me = await bot.GetMeAsync();
Console.Title = me.Username ?? "My awesome Bot";

using var cts = new CancellationTokenSource();

HandlersService handlersService = new HandlersService(new ParserServices(_fileConfig));

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