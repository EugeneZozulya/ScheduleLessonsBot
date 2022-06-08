using System.Configuration;

namespace KHAIScheduleBot
{
    public class BotConfig : IBotConfig
    {
        public string BotToken => ConfigurationManager.AppSettings["token"];

        public string FileDataFileName => ConfigurationManager.AppSettings["dataFileName"];

        public string BotName => ConfigurationManager.AppSettings["botName"];
    }
}
