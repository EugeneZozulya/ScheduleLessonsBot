using System;
using System.Collections.Generic;
using System.Text;

namespace KHAIScheduleBot.Models
{
    public class Couple
    {
        public string Time { get; set; } = string.Empty;
        public List<CoupleType> Types { get; set; } = new List<CoupleType>();
        public override string ToString()
        {
            return $"🕔{this.Time}🕔\n\n" + string.Join("\n------------------------------------\n", this.Types) + '\n';
        }
    }
}
