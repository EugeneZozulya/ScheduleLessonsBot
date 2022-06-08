using System.Collections.Generic;

namespace KHAIScheduleBot.Models
{
    public class CoupleType
    {
        /// <summary>
        /// Classroom where the couple passes.
        /// </summary>
        public string Classroom { get; set; } = string.Empty;
        /// <summary>
        /// Discipline, witch is passed.
        /// </summary>
        public Discipline Discipline { get; set; } = new Discipline();
        /// <summary>
        /// Teacher who reads discipline.
        /// </summary>
        public List<Teacher> Teachers { get; set; } = new List<Teacher>();
        /// <summary>
        /// Convert CoupleType object to the string.
        /// </summary>
        /// <returns> String representation of an CoupleType object. </returns>
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
