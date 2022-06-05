namespace KHAIScheduleBot.Models
{
    public class Teacher
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{this.Category} {this.FullName}";
        }
    }
}
