using RandomStudentPicker.Models;
using RandomStudentPicker.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RandomStudentPicker.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly FileService _fileService = new FileService();
        private ObservableCollection<Student> _students = new ObservableCollection<Student>();
        private ObservableCollection<string> _classList;

        public MainPage()
        {
            InitializeComponent();
            _classList = new ObservableCollection<string>();
            ClassPicker.ItemsSource = _classList;
            StudentsList.ItemsSource = _students;
            LoadClasses();
        }

        private void LoadClasses()
        {
            var classNames = _fileService.GetClassNames();
            foreach (var className in classNames)
            {
                _classList.Add(className);
            }

            if (classNames.Any())
            {
                ClassPicker.SelectedIndex = 0;
                LoadStudentsForClass(classNames.First());
            }
        }

        private void LoadStudentsForClass(string className)
        {
            _students.Clear();
            var loadedStudents = _fileService.LoadClassList(className);
            foreach (var student in loadedStudents)
            {
                _students.Add(student);
            }
        }

        private void OnClassSelected(object sender, EventArgs e)
        {
            if (ClassPicker.SelectedItem is string className)
            {
                LoadStudentsForClass(className);
            }
        }

        private void OnAddStudentClicked(object sender, EventArgs e)
        {
            string studentName = StudentNameEntry.Text?.Trim();
            string journalNumberText = JournalNumberEntry.Text?.Trim();
            string className = ClassPicker.SelectedItem as string;

            // Sprawdzamy, czy wszystkie dane są poprawne
            if (string.IsNullOrEmpty(studentName))
            {
                DisplayAlert("Error", "Student name cannot be empty.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(journalNumberText))
            {
                DisplayAlert("Error", "Journal number cannot be empty.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(className))
            {
                DisplayAlert("Error", "Please select a class.", "OK");
                return;
            }

            // Sprawdzamy, czy numer indeksu jest liczbą
            if (!int.TryParse(journalNumberText, out int journalNumber))
            {
                DisplayAlert("Error", "Invalid journal number.", "OK");
                return;
            }

            // Dodajemy studenta do listy
            _students.Add(new Student { Name = studentName, ClassName = className, JournalNumber = journalNumber });

            // Czyszczenie pól tekstowych po dodaniu studenta
            StudentNameEntry.Text = string.Empty;
            JournalNumberEntry.Text = string.Empty;

            // Opcjonalnie: W przypadku, gdyby lista nie była aktualizowana, wywołujemy:
            StudentsList.ItemsSource = _students; // Upewnijmy się, że lista jest zaktualizowana
        }


        private void OnRemoveStudentClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Student student)
            {
                _students.Remove(student);
            }
        }

        private void OnSaveClassClicked(object sender, EventArgs e)
        {
            string className = ClassPicker.SelectedItem as string;
            if (!string.IsNullOrEmpty(className))
            {
                _fileService.SaveClassList(className, _students.ToList());
                DisplayAlert("Success", "Class saved!", "OK");
            }
        }

        private async void OnPickStudentClicked(object sender, EventArgs e)
        {
            if (_students.Any())
            {
                var random = new Random();
                int luckyNumber = random.Next(1, _students.Count + 1);
                foreach (var student in _students) { student.IsLucky = false; }
                _students[luckyNumber - 1].IsLucky = true;

                var eligibleStudents = _students.Where(s => !s.IsLucky).ToList();
                if (eligibleStudents.Any())
                {
                    // Picking animation loop
                    for (int i = 0; i < 5; i++)
                    {
                        var tempStudent = eligibleStudents[random.Next(eligibleStudents.Count)];
                        PickedStudentLabel.Text = $"Picking: {tempStudent.JournalNumber} - {tempStudent.Name}";

                        // Animate scaling and rotation with bounce effect
                        var scaleAnimation = new Animation(v => PickedStudentLabel.Scale = v, 1, 1.5);  // Zoom in
                        var rotationAnimation = new Animation(v => PickedStudentLabel.Rotation = v, 0, 360);  // Rotate
                        var opacityAnimation = new Animation(v => PickedStudentLabel.Opacity = v, 1, 0.5);  // Fade out
                        var bounceAnimation = new Animation(v => PickedStudentLabel.Scale = v, 1.5, 1);  // Bounce back

                        // Combine the animations into a single animation
                        var combinedAnimation = new Animation();
                        combinedAnimation.Add(0, 0.3, scaleAnimation);
                        combinedAnimation.Add(0, 0.3, rotationAnimation);
                        combinedAnimation.Add(0, 0.5, opacityAnimation);
                        combinedAnimation.Add(0.5, 1, bounceAnimation);

                        combinedAnimation.Commit(PickedStudentLabel, "PickingAnimation", 16, 1000, Easing.BounceOut);

                        await Task.Delay(200);  // Delay between picking cycles
                    }

                    // After the picking animation, show the selected student
                    var pickedStudent = eligibleStudents[random.Next(eligibleStudents.Count)];
                    PickedStudentLabel.Text = $"Picked: {pickedStudent.JournalNumber} - {pickedStudent.Name}";

                    // Final bounce animation
                    var finalAnimation = new Animation(v => PickedStudentLabel.Scale = v, 1.5, 1);  // Bounce back
                    finalAnimation.Commit(PickedStudentLabel, "FinalBounce", 16, 500, Easing.BounceOut);
                }
            }
        }


        // Custom BounceInOut Easing function
        public static class CustomEasing
        {
            public static Easing BounceInOut = new Easing((t) =>
            {
                if (t < 0.5)
                {
                    return (7.5625 * t * t);
                }
                t -= 1;
                return (7.5625 * t * t + 0.75);
            });
        }

        private void OnCreateClassClicked(object sender, EventArgs e)
        {
            string newClassName = NewClassEntry.Text?.Trim();

            if (string.IsNullOrEmpty(newClassName))
            {
                DisplayAlert("Error", "Class name cannot be empty.", "OK");
                return;
            }

            if (_classList.Contains(newClassName))
            {
                DisplayAlert("Error", "Class already exists.", "OK");
                return;
            }

            _classList.Add(newClassName);
            NewClassEntry.Text = string.Empty;

            // Save the new class to file (assuming FileService has a method to persist class names)
            _fileService.SaveClassNames(_classList.ToList());
            System.Diagnostics.Debug.WriteLine($"Created new class: {newClassName}");
        }
    }
}
