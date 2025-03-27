using RandomStudentPicker.Models;
using RandomStudentPicker.Services;

namespace RandomStudentPicker.Views
{
    public partial class StudentListPage : ContentPage
    {
        private readonly FileService _fileService;
        private Class _currentClass;

        public StudentListPage(string className)
        {
            InitializeComponent();
            _fileService = new FileService();
            _currentClass = _fileService.LoadClass(className) ?? new Class { ClassName = className };
            LoadStudents();
        }

        private void LoadStudents()
        {
            StudentsCollectionView.ItemsSource = _currentClass.Students.ToList();
        }

        private void OnAddStudentClicked(object sender, EventArgs e)
        {
            var firstName = FirstNameEntry.Text?.Trim();
            var lastName = LastNameEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                DisplayAlert("B³¹d", "WprowadŸ imiê i nazwisko ucznia", "OK");
                return;
            }

            _currentClass.Students.Add(new Student
            {
                FirstName = firstName,
                LastName = lastName
            });

            FirstNameEntry.Text = string.Empty;
            LastNameEntry.Text = string.Empty;
            LoadStudents();
        }

        private void OnDeleteStudentClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var student = (Student)button.BindingContext;

            _currentClass.Students.Remove(student);
            LoadStudents();
        }

        private async void OnSaveChangesClicked(object sender, EventArgs e)
        {
            _fileService.SaveClass(_currentClass);
            await DisplayAlert("Sukces", "Zmiany zosta³y zapisane", "OK");
            await Navigation.PopAsync();
        }
    }
}