using System.Collections.Generic;

namespace KHAIScheduleBot.Models
{
    public class Day
    {
        /// <summary>
        /// Day's name.
        /// </summary>
        public string DayName { get; set; } = string.Empty;
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
            return $"            📅{this.DayName}📅\n\n" + string.Join('\n', this.Couples);
        }
    }
}
