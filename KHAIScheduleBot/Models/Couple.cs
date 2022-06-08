using System;
using System.Collections.Generic;
using System.Text;

namespace KHAIScheduleBot.Models
{
    public class Couple
    {
        /// <summary>
        /// The time the couple passes.
        /// </summary>
        public string Time { get; set; } = string.Empty;
        /// <summary>
        /// Types of this couple, max can be 2 when week is denominator and when week is numerator.
        /// </summary>
        public List<CoupleType> Types { get; set; } = new List<CoupleType>();
        /// <summary>
        /// Convert Couple object to the string.
        /// </summary>
        /// <returns> String representation of an Couple object. </returns>
        public override string ToString()
        {
            return $"🕔{this.Time}🕔\n\n" + string.Join("\n------------------------------------\n", this.Types) + '\n';
        }
    }
}
