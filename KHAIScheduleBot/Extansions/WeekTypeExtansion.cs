using KHAIScheduleBot.Models;

namespace KHAIScheduleBot.Extansions
{
    static class WeekTypeExtansion
    {
        /// <summary>
        /// Convert WeekType object to string.
        /// </summary>
        /// <param name="week"> WeekType Object. </param>
        /// <returns>String represent of the WeekType object.</returns>
        public static string GetString(this WeekType week)
        {
            string weekString = string.Empty;

            switch (week)
            {
                case WeekType.Denomanator: weekString = "Знаменник"; break;
                case WeekType.Numerator: weekString = "Чисельник"; break;
                case WeekType.Both: weekString = "Обидва"; break;
            }

            return weekString;
        }
    }
}
