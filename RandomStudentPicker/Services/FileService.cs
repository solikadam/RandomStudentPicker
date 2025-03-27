using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using RandomStudentPicker.Models;

namespace RandomStudentPicker.Services
{
    public class FileService
    {
        private readonly string _basePath = FileSystem.AppDataDirectory;

        public void SaveClass(Class classModel)
        {
            var filePath = Path.Combine(_basePath, $"{classModel.ClassName}.txt");
            var lines = classModel.Students.Select(s => $"{s.FirstName},{s.LastName}");
            File.WriteAllLines(filePath, lines);
        }

        public Class LoadClass(string className)
        {
            var filePath = Path.Combine(_basePath, $"{className}.txt");
            if (!File.Exists(filePath)) return null;

            var lines = File.ReadAllLines(filePath);
            var students = new List<Student>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');

                
                if (parts.Length >= 2)
                {
                    students.Add(new Student
                    {
                        FirstName = parts[0].Trim(),
                        LastName = parts[1].Trim()
                    });
                }
                
                else if (parts.Length == 1)
                {
                    students.Add(new Student
                    {
                        FirstName = string.Empty,
                        LastName = parts[0].Trim()
                    });
                }
            }

            return new Class { ClassName = className, Students = students };
        }

        public List<string> GetAvailableClasses()
        {
            return Directory.GetFiles(_basePath, "*.txt")
                           .Select(Path.GetFileNameWithoutExtension)
                           .ToList();
        }
    }

}
