using System.Configuration;

namespace KHAIScheduleBot
{
    /// <summary>
    /// Class which represetns config file.
    /// </summary>
    public class BotConfig : IBotConfig
    {
        /// <summary>
        /// Bot's token.
        /// </summary>
        public string BotToken => ConfigurationManager.AppSettings["token"];
        /// <summary>
        /// Path to file with data.
        /// </summary>
        public string FileDataFileName => ConfigurationManager.AppSettings["dataFileName"];
        /// <summary>
        /// Bot's name.
        /// </summary>
        public string BotName => ConfigurationManager.AppSettings["botName"];
    }
}
