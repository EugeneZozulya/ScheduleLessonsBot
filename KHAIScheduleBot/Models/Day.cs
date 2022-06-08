using System.Collections.Generic;
using KHAIScheduleBot.Extansions;

namespace KHAIScheduleBot.Models
{
    public class Day
    {
        /// <summary>
        /// Day's name.
        /// </summary>
        public DayType DayName { get; set; } = DayType.None;
        /// <summary>
        /// Couples on this day.
        /// </summary>
        public List<Couple> Couples { get; set; } = new List<Couple>();
        /// <summary>
        /// Convert Day object to the string.
        /// </summary>
        /// <returns> String representation of an Day object. </returns>
        public override string ToString()
        {
            return $"            📅{this.DayName.GetString()}📅\n\n" + string.Join('\n', this.Couples);
        }
    }
}
