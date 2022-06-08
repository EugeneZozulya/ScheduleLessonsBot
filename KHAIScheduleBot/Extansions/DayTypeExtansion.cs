using KHAIScheduleBot.Models;

namespace KHAIScheduleBot.Extansions
{
    static class DayTypeExtansion
    {
        /// <summary>
        /// Convert DayType object to string.
        /// </summary>
        /// <param name="day"> DayType Object. </param>
        /// <returns>String represent of the DayType object.</returns>
        public static string GetString(this DayType day)
        {
            string dayString = string.Empty;

            switch(day){
                case DayType.Monday: dayString = "Понеділок"; break;
                case DayType.Tuesday: dayString = "Вівторок"; break;
                case DayType.Wednesday: dayString = "Середа"; break;
                case DayType.Thursday: dayString = "Четвер"; break;
                case DayType.Friday: dayString = "П'ятниця"; break;
            }

            return dayString;
        }
    }
}
