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

        public FileService()
        {
            Task.Run(InitializePredefinedClassesFromFileAsync).Wait();
        }

        // Inicjalizowanie klas ze wstępnie zdefiniowanego pliku
        private async Task InitializePredefinedClassesFromFileAsync()
        {
            System.Diagnostics.Debug.WriteLine("Starting initialization from PredefinedClasses.txt");

            try
            {
                string filePath = "Raw/PredefinedClasses.txt";  // Ścieżka do zasobu w pakiecie aplikacji

                System.Diagnostics.Debug.WriteLine($"Looking for PredefinedClasses.txt in resources at {filePath}");

                using var stream = await FileSystem.OpenAppPackageFileAsync(filePath); // Dostęp do pliku z zasobów aplikacji

                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine("PredefinedClasses.txt not found in resources!");
                    return;
                }

                using var reader = new StreamReader(stream);
                string content = await reader.ReadToEndAsync();
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                string currentClassName = null;
                var students = new List<Student>();

                // Przetwarzanie wierszy z pliku
                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    System.Diagnostics.Debug.WriteLine($"Processing: '{trimmedLine}'");

                    if (string.IsNullOrEmpty(trimmedLine))
                        continue;

                    if (trimmedLine.StartsWith("#"))
                    {
                        if (currentClassName != null && students.Count > 0)
                        {
                            SaveClassList(currentClassName, students);
                            System.Diagnostics.Debug.WriteLine($"Saved class: {currentClassName} with {students.Count} students");
                            students.Clear();
                        }
                        currentClassName = trimmedLine.Substring(1);
                    }
                    else if (currentClassName != null)
                    {
                        var parts = trimmedLine.Split(';');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int journalNumber))
                        {
                            students.Add(new Student
                            {
                                JournalNumber = journalNumber,
                                Name = parts[1].Trim(),
                                ClassName = currentClassName
                            });
                            System.Diagnostics.Debug.WriteLine($"Added: {journalNumber};{parts[1]} to {currentClassName}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Invalid line format: '{trimmedLine}'");
                        }
                    }
                }

                if (currentClassName != null && students.Count > 0)
                {
                    SaveClassList(currentClassName, students);
                    System.Diagnostics.Debug.WriteLine($"Saved final class: {currentClassName} with {students.Count} students");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading PredefinedClasses.txt: {ex.Message}");
            }
        }

        // Zapis klasy do lokalnego pliku
        public void SaveClassList(string className, List<Student> students)
        {
            // Utworzenie pełnej ścieżki do pliku w AppData
            string filePath = Path.Combine(_basePath, $"{className}.txt");
            var lines = students.Select(s => $"{s.JournalNumber};{s.Name}").ToList();

            // Zapisanie do pliku
            File.WriteAllLines(filePath, lines);
            System.Diagnostics.Debug.WriteLine($"Saved {students.Count} students to {filePath}");
        }

        // Wczytanie listy studentów z pliku klasy
        public List<Student> LoadClassList(string className)
        {
            string filePath = Path.Combine(_basePath, $"{className}.txt");
            System.Diagnostics.Debug.WriteLine($"Trying to load class list from {filePath}");

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                var students = new List<Student>();
                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int journalNumber))
                    {
                        students.Add(new Student
                        {
                            JournalNumber = journalNumber,
                            Name = parts[1].Trim(),
                            ClassName = className
                        });
                    }
                }
                System.Diagnostics.Debug.WriteLine($"Loaded {students.Count} students from {filePath}");
                return students;
            }
            System.Diagnostics.Debug.WriteLine($"No file found for {className}");
            return new List<Student>();
        }

        // Pobranie nazw klas dostępnych w lokalnym katalogu
        public List<string> GetClassNames()
        {
            string filePath = Path.Combine(_basePath, "ClassNames.txt");

            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath).ToList();
            }

            return new List<string>();
        }

        public void SaveClassNames(List<string> classNames)
        {
            string filePath = Path.Combine(_basePath, "ClassNames.txt");
            File.WriteAllLines(filePath, classNames);
        }
    }
}
