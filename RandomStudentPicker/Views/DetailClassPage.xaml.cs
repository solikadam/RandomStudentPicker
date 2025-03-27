using System;
using System.Collections.ObjectModel;
using System.Linq;
using RandomStudentPicker.Models;
using RandomStudentPicker.Services;

namespace RandomStudentPicker.Views;

public partial class DetailClassPage : ContentPage
{
    public string ClassName { get; private set; }
    public ObservableCollection<Student> Students { get; private set; } = new();
    public ObservableCollection<Student> StudentsForRandomSelection { get; private set; } = new();
    private LinkedList<string> RecentlyDrawnStudents { get; set; } = new();
    private const int MaxRecentDraws = 3;

    public bool IsRandomSelectionVisible => RandomSelectionList.IsVisible;

    public DetailClassPage(string className)
    {
        InitializeComponent();
        ClassName = className;
        Title = $"Klasa: {ClassName}";
        BindingContext = this;
        LoadStudentsAsync();
    }

    private async void LoadStudentsAsync()
    {
        try
        {
            var studentNames = await FileService.GetStudentsAsync(ClassName);
            Students.Clear();
            StudentsForRandomSelection.Clear();

            foreach (var name in studentNames)
            {
                var student = new Student { Name = name, IsSelected = true };
                Students.Add(student);
                StudentsForRandomSelection.Add(student);
            }

            StudentsList.ItemsSource = Students;
        }
        catch (Exception ex)
        {
            await DisplayAlert("B��d", $"Nie uda�o si� za�adowa� listy uczni�w.\n{ex.Message}", "OK");
        }
    }

    private async void OnAddStudentClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Nowy Ucze�", "Podaj nazw� ucznia:");
        if (!string.IsNullOrWhiteSpace(result))
        {
            await FileService.AddStudentAsync(ClassName, result.Trim());
            LoadStudentsAsync();
        }
    }

    private async void OnStudentTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is not Student selectedStudent)
            return;

        string action = await DisplayActionSheet(selectedStudent.Name, "Anuluj", null, "Edytuj", "Usu�");

        if (action == "Edytuj")
        {
            string newName = await DisplayPromptAsync("Edytuj Ucznia", "Podaj now� nazw�:", initialValue: selectedStudent.Name);
            if (!string.IsNullOrWhiteSpace(newName))
            {
                await FileService.UpdateStudentAsync(ClassName, selectedStudent.Name, newName.Trim());
                LoadStudentsAsync();
            }
        }
        else if (action == "Usu�")
        {
            bool confirm = await DisplayAlert("Usu� Ucznia", $"Czy na pewno chcesz usun�� {selectedStudent.Name}?", "Tak", "Nie");
            if (confirm)
            {
                await FileService.DeleteStudentAsync(ClassName, selectedStudent.Name);
                LoadStudentsAsync();
            }
        }

        ((ListView)sender).SelectedItem = null;
    }

    private void OnStartRandomSelectionClicked(object sender, EventArgs e)
    {
        if (!StudentsForRandomSelection.Any())
        {
            DisplayAlert("Brak uczni�w", "Lista uczni�w jest pusta. Dodaj uczni�w, aby kontynuowa�.", "OK");
            return;
        }

        RandomSelectionList.ItemsSource = StudentsForRandomSelection;
        RandomSelectionList.IsVisible = true;
        RandomButton.IsVisible = true;
    }

    private async void OnPerformRandomSelectionClicked(object sender, EventArgs e)
    {
        var selectedStudents = StudentsForRandomSelection.Where(s => s.IsSelected).Select(s => s.Name).ToList();
        if (!selectedStudents.Any())
        {
            await DisplayAlert("Brak Wyboru", "Nie wybrano �adnych uczni�w do losowania.", "OK");
            return;
        }

        var availableStudents = selectedStudents.Except(RecentlyDrawnStudents).ToList();
        if (!availableStudents.Any())
        {
            await DisplayAlert("Brak dost�pnych uczni�w", "Wszyscy zaznaczeni uczniowie zostali wykluczeni z losowania na 3 rundy.", "OK");
            RecentlyDrawnStudents.Clear();
            availableStudents = selectedStudents;
        }

        var random = new Random();
        string randomStudent = availableStudents[random.Next(availableStudents.Count)];

        RecentlyDrawnStudents.AddLast(randomStudent);
        if (RecentlyDrawnStudents.Count > MaxRecentDraws)
        {
            RecentlyDrawnStudents.RemoveFirst();
        }

        await DisplayAlert("Wylosowany Ucze�", $"Wylosowano: {randomStudent}", "OK");

        await FileService.SaveRoundHistoryAsync(ClassName, selectedStudents, RecentlyDrawnStudents.ToList());

        RandomSelectionList.IsVisible = false;
        RandomButton.IsVisible = false;
    }
}