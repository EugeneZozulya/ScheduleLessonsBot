using System.Configuration;

namespace KHAIScheduleBot
{
    public class FileConfig : IFileConfig
    {
        public string FileToken => ConfigurationManager.AppSettings["token"];

        public string FileDataFileName => ConfigurationManager.AppSettings["dataFileName"];
    }
}
