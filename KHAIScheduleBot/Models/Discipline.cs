namespace KHAIScheduleBot.Models
{
    public class Discipline
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public override string ToString()
        {
            return $"{this.Title}";
        }
    }
}
