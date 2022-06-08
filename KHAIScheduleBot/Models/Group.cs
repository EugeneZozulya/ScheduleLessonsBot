using System.Collections.Generic;

namespace KHAIScheduleBot.Models
{
    public class Group
    {
        /// <summary>
        /// Group's id.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Days for this group with the schedule.
        /// </summary>
        public List<Day> Days { get; set; } = new List<Day>();
        /// <summary>
        /// Convert Group object to the string.
        /// </summary>
        /// <returns> String representation of an Group object. </returns>
        public override string ToString()
        {
            return "```" + string.Join("\n\n", this.Days) + "```";
        }
    }
}
