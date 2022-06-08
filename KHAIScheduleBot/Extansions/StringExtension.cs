using KHAIScheduleBot.Models;

namespace KHAIScheduleBot.Extansions
{
    static class StringExtension
    {
        /// <summary>
        /// Convert string to DayType object.
        /// </summary>
        /// <param name="dayString"> String represent of the DayType object. </param>
        /// <returns>DayType Object.</returns>
        public static DayType GetDayType(this string dayString)
        {
            DayType day = DayType.None;

            switch (dayString)
            {
                case "Понеділок": day = DayType.Monday; break;
                case "Вівторок": day = DayType.Tuesday; break;
                case "Середа": day = DayType.Wednesday; break;
                case "Четвер": day = DayType.Thursday; break;
                case "П'ятниця": day = DayType.Friday; break;
            }

            return day;
        }
        /// <summary>
        /// Convert string to WeekType object.
        /// </summary>
        /// <param name="weekString"> String represent of the WeekType object. </param>
        /// <returns>WeekType Object.</returns>
        public static WeekType GetWeekType(this string weekString)
        {
            WeekType week = WeekType.None;

            switch (weekString)
            {
                case "Чисельник": week = WeekType.Numerator; break;
                case "Знаменник": week = WeekType.Denomanator; break;
                case "Обидва": week = WeekType.Both; break;
            }

            return week;
        }
    }
}
