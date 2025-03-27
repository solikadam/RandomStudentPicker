using RandomStudentPicker.Models;
using RandomStudentPicker.Services;
using RandomStudentPicker.Views;

namespace RandomStudentPicker.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly FileService _fileService;
        private readonly RandomPickerService _randomPickerService;
        private Class _currentClass;

        public MainPage()
        {
            InitializeComponent();
            _fileService = new FileService();
            _randomPickerService = new RandomPickerService();
            LoadAvailableClasses();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadAvailableClasses();
        }

        private void LoadAvailableClasses()
        {
            ClassPicker.ItemsSource = _fileService.GetAvailableClasses();
        }

        private async void OnLoadClassClicked(object sender, EventArgs e)
        {
            if (ClassPicker.SelectedItem == null)
            {
                await DisplayAlert("Błąd", "Wybierz klasę z listy", "OK");
                return;
            }

            var className = ClassPicker.SelectedItem.ToString();
            _currentClass = _fileService.LoadClass(className);
            ResultLabel.Text = $"Wczytano klasę: {className} ({_currentClass.Students.Count} uczniów)";
        }

        private void OnPickRandomClicked(object sender, EventArgs e)
        {
            if (_currentClass == null || _currentClass.Students.Count == 0)
            {
                ResultLabel.Text = "Najpierw wczytaj klasę z uczniami";
                return;
            }

            var student = _randomPickerService.PickRandomStudent(_currentClass);
            ResultLabel.Text = $"Wylosowany uczeń: {student}";
        }

        private async void OnManageClassesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ClassManagementPage());
        }
    }
}