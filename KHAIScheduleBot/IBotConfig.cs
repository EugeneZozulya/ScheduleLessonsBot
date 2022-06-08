namespace KHAIScheduleBot
{
    /// <summary>
    /// Contract for reading config file.
    /// </summary>
    public interface IBotConfig
    {
        /// <summary>
        /// Bot's token.
        /// </summary>
        string BotToken { get; }
        /// <summary>
        /// Path to file with data.
        /// </summary>
        string FileDataFileName { get; }
        /// <summary>
        /// Bot's name.
        /// </summary>
        string BotName { get; }
    }
}
