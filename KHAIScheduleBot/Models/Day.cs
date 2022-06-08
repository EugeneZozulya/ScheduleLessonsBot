using System.Collections.Generic;

namespace KHAIScheduleBot.Models
{
    public class Day
    {
        public string DayName { get; set; } = string.Empty;
        public List<Couple> Couples { get; set; } = new List<Couple>();
        public override string ToString()
        {
            return $"            📅{this.DayName}📅\n\n" + string.Join('\n', this.Couples);
        }
    }
}
