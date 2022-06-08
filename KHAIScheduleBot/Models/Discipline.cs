namespace KHAIScheduleBot.Models
{
    public class Discipline
    {
        /// <summary>
        /// Disciline's id.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Title of the discipline.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Convert Discipline object to the string.
        /// </summary>
        /// <returns> String representation of an Discipline object. </returns>
        public override string ToString()
        {
            return $"{this.Title}";
        }
    }
}
