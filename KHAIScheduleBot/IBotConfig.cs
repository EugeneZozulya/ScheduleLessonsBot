namespace KHAIScheduleBot
{
    public interface IBotConfig
    {
        string BotToken { get; }
        string FileDataFileName { get; }
        string BotName { get; }
    }
}
