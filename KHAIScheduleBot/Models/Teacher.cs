namespace KHAIScheduleBot.Models
{
    public class Teacher
    {
        /// <summary>
        /// Teacher's id.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Teacher's fullname.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Teacher's category.
        /// </summary>
        public string Category { get; set; } = string.Empty;
        /// <summary>
        /// Convert Teacher object to the string.
        /// </summary>
        /// <returns> String representation of an Teacher object. </returns>
        public override string ToString()
        {
            return $"{this.Category} {this.FullName}";
        }
    }
}
