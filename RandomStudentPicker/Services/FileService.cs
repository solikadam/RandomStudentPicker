using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using RandomStudentPicker.Models;

namespace RandomStudentPicker.Services
{
    public static class FileService
    {
        private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static string ClassesFilePath => Path.Combine(LocalAppData, "Classes.txt");

        private static string GetFilePath(string fileName) => Path.Combine(LocalAppData, fileName);

        public static async Task<List<string>> GetClassesAsync()
        {
            return await ReadFileLinesAsync(ClassesFilePath);
        }

        public static async Task AddClassAsync(string className)
        {
            await AppendLineToFileAsync(ClassesFilePath, className);
        }

        public static async Task DeleteClassAsync(string className)
        {
            await RemoveLineFromFileAsync(ClassesFilePath, className);
            File.Delete(GetFilePath($"Students_{className}.txt"));
            File.Delete(GetFilePath($"RoundHistory_{className}.txt"));
        }

        public static async Task<List<string>> GetStudentsAsync(string className)
        {
            return await ReadFileLinesAsync(GetFilePath($"Students_{className}.txt"));
        }

        public static async Task AddStudentAsync(string className, string studentName)
        {
            await AppendLineToFileAsync(GetFilePath($"Students_{className}.txt"), studentName);
        }

        public static async Task UpdateStudentAsync(string className, string oldName, string newName)
        {
            await ReplaceLineInFileAsync(GetFilePath($"Students_{className}.txt"), oldName, newName);
        }

        public static async Task DeleteStudentAsync(string className, string studentName)
        {
            await RemoveLineFromFileAsync(GetFilePath($"Students_{className}.txt"), studentName);
        }

        public static async Task SaveRoundHistoryAsync(string className, List<string> participatingStudents, List<string> excludedStudents)
        {
            string filePath = GetFilePath($"RoundHistory_{className}.txt");
            string historyEntry = $"Runda: {DateTime.Now}\n" +
                                  $"Uczestnicy: {string.Join(", ", participatingStudents)}\n" +
                                  $"Wykluczeni: {string.Join(", ", excludedStudents)}\n";
                                 
            await AppendTextToFileAsync(filePath, historyEntry);
        }

        private static async Task<List<string>> ReadFileLinesAsync(string filePath)
        {
            return File.Exists(filePath) ? (await File.ReadAllLinesAsync(filePath)).ToList() : new List<string>();
        }

        private static async Task AppendLineToFileAsync(string filePath, string line)
        {
            if (!File.Exists(filePath) || !(await File.ReadAllLinesAsync(filePath)).Contains(line))
            {
                await File.AppendAllTextAsync(filePath, line + "\n");
            }
        }

        private static async Task RemoveLineFromFileAsync(string filePath, string lineToRemove)
        {
            if (!File.Exists(filePath)) return;
            var lines = (await File.ReadAllLinesAsync(filePath)).Where(line => line != lineToRemove).ToList();
            await File.WriteAllLinesAsync(filePath, lines);
        }

        private static async Task ReplaceLineInFileAsync(string filePath, string oldLine, string newLine)
        {
            if (!File.Exists(filePath)) return;
            var lines = (await File.ReadAllLinesAsync(filePath)).Select(line => line == oldLine ? newLine : line).ToList();
            await File.WriteAllLinesAsync(filePath, lines);
        }

        private static async Task AppendTextToFileAsync(string filePath, string text)
        {
            await File.AppendAllTextAsync(filePath, text);
        }
    }

}
