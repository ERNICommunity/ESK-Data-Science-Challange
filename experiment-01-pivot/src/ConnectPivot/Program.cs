using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

using Line = System.Collections.Generic.Dictionary<string, string>;

namespace ConnectPivot
{
    class Program
    {
        private const string InputDir = @"C:\Workspace\ESK-Data-Science-Challange\input";        
        //private static string EducationFile = Path.Combine(InputDir, "education.csv");
        private static string KnowHowFile = Path.Combine(InputDir, "know-how.csv");
        private static string LanguageFile = Path.Combine(InputDir, "language.csv");
        private static string PersonFile = Path.Combine(InputDir, "person.csv");
        private static string ProjectFile = Path.Combine(InputDir, "project.csv");
        //private static string ProjectSkillFile = Path.Combine(InputDir, "project-skill.csv");
        //private static string ProjectTaskFile = Path.Combine(InputDir, "project-task.csv");

        private const string OutputFile = @"C:\Workspace\ESK-Data-Science-Challange\experiment-01-pivot\features.csv";

        static void Main(string[] args)
        {
            var knowhow = ReadFile(KnowHowFile);
            var language = ReadFile(LanguageFile);
            var person = ReadFile(PersonFile);
            //var project = ReadFile(ProjectFile).Where(p => p["Internal"] == "1").ToList();

            var langs = language.Select(line => line["Language"]).Distinct().OrderBy(self => self).ToList();
            var langKeys = langs.ToDictionary(self => self, l => String.Format("Language[\"{0}\"]", l));

            var khs = knowhow.Select(line => line["KnowHow"]).Distinct().OrderBy(self => self).ToList();
            var khsKeys = khs.ToDictionary(self => self, kh => String.Format("Know-How[\"{0}\"]", kh));
            var khHash = new HashSet<Tuple<string, string>>(knowhow.Select(kh => Tuple.Create(kh["PersonId"], kh["KnowHow"])));

            var pivot = new List<Line>();

            foreach (var per in person)
            {
                var piv = new Line(per);
                pivot.Add(piv);

                foreach (var lang in langs)
                {
                    var level = language
                        .Where(item => item["PersonId"] == per["Id"])
                        .Where(item => item["Language"] == lang)
                        .Select(item => item["Level"])
                        .FirstOrDefault();
                    piv[langKeys[lang]] = level;
                }

                foreach (var kh in khs)
                {
                    var hasKh = khHash.Contains(Tuple.Create(per["Id"], kh));
                    piv[khsKeys[kh]] = hasKh ? "1" : "0";
                }
            }

            var pivotHeaders = pivot.First().Keys;

            using (var stream = new StreamWriter(OutputFile, false, Encoding.UTF8))
            using (var csv = new CsvWriter(stream))
            {
                foreach (var header in pivotHeaders)
                {
                    csv.WriteField(header);
                }

                csv.NextRecord();

                foreach (var line in pivot)
                {
                    foreach (var header in pivotHeaders)
                    {
                        csv.WriteField(line[header]);
                    }

                    csv.NextRecord();
                }
            }
        }

        private static List<Line> ReadFile(string filePath)
        {
            var lines = new List<Line>();
            using (var stream = new StreamReader(filePath, Encoding.UTF8))
            using (var csv = new CsvReader(stream))
            {
                csv.ReadHeader();
                var header = csv.FieldHeaders;

                while (csv.Read())
                {
                    var line = new Line();
                    foreach (var name in header)
                    {
                        line[name] = csv.GetField(name);
                    }
                    lines.Add(line);
                }
            }
            return lines;
        }
    }
}
