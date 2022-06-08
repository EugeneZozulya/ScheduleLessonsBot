using KHAIScheduleBot.Models;
namespace KHAIScheduleBot.Services
{    
    /// <summary>
    /// Contract for parsing schedule from the file.
    /// </summary>
    public interface IParserService
    {
        /// <summary>
        /// Get schedule of the couples for some group.
        /// </summary>
        /// <param name="groupId">Group's id</param>
        /// <param name="dayOfWeek">Some day of week, such as: Monday, Tuesday, Wednesday, Thursday, Friday, None - all week.</param>
        /// <param name="typeWeek">Some type of week, such as: denominator, numerator, None - both type.</param>
        /// <returns>Group class object with parsed schedule from file. </returns>
        Group GetSchedule(string groupId, DayType dayOfWeek = DayType.None, WeekType typeWeek = WeekType.None);
        /// <summary>
        /// Get all groups id.
        /// </summary>
        /// <returns>List<string> with groups id if they are, otherwise - null.</returns>
        List<String> GetAllGroupsId();
    }
}
