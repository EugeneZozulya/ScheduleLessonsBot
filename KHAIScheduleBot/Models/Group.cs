using System.Collections.Generic;

namespace KHAIScheduleBot.Models
{
    public class Group
    {
        public string Id { get; set; } = string.Empty;
        public List<Day> Days { get; set; } = new List<Day>();
        public override string ToString()
        {
            return string.Join('\n', this.Days);
        }
    }
}
