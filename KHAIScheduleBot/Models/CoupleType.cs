using System.Collections.Generic;

namespace KHAIScheduleBot.Models
{
    public class CoupleType
    {
        public string Classroom { get; set; } = string.Empty;
        public Discipline Discipline { get; set; } = new Discipline();
        public List<Teacher> Teachers { get; set; } = new List<Teacher>();

        public override string ToString()
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(Classroom))
                result = "😎Пари не має😎";
            else
                result = $"{this.Classroom} {this.Discipline}\n" + string.Join('\n', this.Teachers);
            return result;
        }
    }
}
