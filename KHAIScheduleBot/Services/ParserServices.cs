using System.Xml;
using KHAIScheduleBot.Models;
using System.Collections.Generic;

namespace KHAIScheduleBot.Services
{
    /// <summary>
    /// Parse schedule from the file.
    /// </summary>
    public class ParserServices
    {
        /// <summary>
        /// Loaded xml document. 
        /// </summary>
        private XmlDocument xmlDocument;
        /// <summary>
        /// IFileConfig object for read application config file.
        /// </summary>
        private IFileConfig _fileConfig;

        /// <summary>
        /// Constructor for initializing a parser. It gets filename from app.config.
        /// </summary>
        /// <param name="fileConfig">IFileConfig object for read application config file.</param>
        public ParserServices(IFileConfig fileConfig)
        {
            _fileConfig = fileConfig;
            xmlDocument = new XmlDocument();
            string path = _fileConfig.FileDataFileName;
            xmlDocument.Load(path);
        }

        /// <summary>
        /// Get schedule of the couples for some group.
        /// </summary>
        /// <param name="groupId">Group's id</param>
        /// <param name="dayOfWeek">Some day of week, such as: Monday, Tuesday, Wednesday, Thursday, Friday, null - all week.</param>
        /// <param name="typeWeek">Some type of week, such as: denominator, numerator, null - both type.</param>
        /// <returns>Group class object with parsed schedule from file. </returns>
        public Group GetSchedule(string groupId, string dayOfWeek = null, string typeWeek = null)
        {
            Group group = new Group();
            XmlElement xmlElement = xmlDocument.GetElementById(groupId);

            //Parse day(s)
            List<Day> days = new List<Day>();
            foreach(XmlNode day in xmlElement.ChildNodes)
            {
                string dayType = day.Attributes["type"].InnerText;

                if (!string.IsNullOrEmpty(dayOfWeek) && dayOfWeek != dayType)
                    continue;

                Day d = new Day();
                d.DayName = dayType;

                //Parse couples
                List<Couple> couples = new List<Couple>();
                foreach(XmlNode couple in day.ChildNodes)
                {
                    Couple c = new Couple();
                    c.Time = couple.FirstChild.InnerText;

                    //Parse couple 
                    List<CoupleType> coupleTypes = new List<CoupleType>();
                    XmlNodeList coupleChilds = couple.ChildNodes[1].ChildNodes;
                    for (int i = 0; i < coupleChilds.Count; i++)
                    {
                        if (i == 0 && coupleChilds.Count == 2 && typeWeek == "Знаменник")
                            continue;
                        if (i == 1 && typeWeek == "Чисельнник")
                            break;
                        CoupleType coupleType = new CoupleType();
                        coupleType.Classroom = coupleChilds[i].FirstChild.InnerText;
                        string disciplineID = coupleChilds[i].ChildNodes[1].InnerText;
                        coupleType.Discipline = GetDiscipline(disciplineID);

                        //Parse teachers
                        List<Teacher> teachers = new List<Teacher>();
                        foreach(XmlNode teacher in coupleChilds[i].LastChild.ChildNodes)
                        {
                            Teacher t =  GetTeacher(teacher.InnerText);
                            teachers.Add(t);
                        }

                        coupleType.Teachers = teachers;
                        coupleTypes.Add(coupleType);
                    }
                    c.Types = coupleTypes;
                    couples.Add(c);
                }

                d.Couples = couples;
                days.Add(d);
            }
            group.Id = xmlElement.GetAttribute("id");
            group.Days = days;
            return group;
        }

        /// <summary>
        /// Get tecaher with some id.
        /// </summary>
        /// <param name="id">Tecaher's id.</param>
        /// <returns>Teacher class object with parsed data from file.</returns>
        Teacher GetTeacher(string id)
        {
            Teacher teacher = new Teacher();
            if (!string.IsNullOrEmpty(id))
            {
                XmlElement xmlElement = xmlDocument.GetElementById(id);
                teacher.Id = xmlElement.GetAttribute("id");
                teacher.FullName = xmlElement.FirstChild.InnerText;
                teacher.Category = xmlElement.LastChild.InnerText;
            }
            return teacher;
        }

        /// <summary>
        /// Get discipline with some id.
        /// </summary>
        /// <param name="id">Discipline's id.</param>
        /// <returns>Disciipline class object with parsed data from file.</returns>
        Discipline GetDiscipline(string id)
        {
            Discipline discipline = new Discipline();
            if (!string.IsNullOrEmpty(id))
            {
                XmlElement xmlElement = xmlDocument.GetElementById(id);
                discipline.Id = xmlElement.GetAttribute("id");
                discipline.Title = xmlElement.FirstChild.InnerText;
            }
            return discipline;
        }
    }
}
