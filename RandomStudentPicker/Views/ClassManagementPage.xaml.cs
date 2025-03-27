using RandomStudentPicker.Models;
using RandomStudentPicker.Services;

namespace RandomStudentPicker.Views
{
    public partial class ClassManagementPage : ContentPage
    {
        private readonly FileService _fileService;

        public ClassManagementPage()
        {
            InitializeComponent();
            _fileService = new FileService();
            LoadClasses();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadClasses();
        }

        private void LoadClasses()
        {
            ClassesCollectionView.ItemsSource = _fileService.GetAvailableClasses()
                .Select(c => new Class { ClassName = c })
                .ToList();
        }

        private async void OnCreateClassClicked(object sender, EventArgs e)
        {
            var className = NewClassNameEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(className))
            {
                await DisplayAlert("B³¹d", "WprowadŸ nazwê klasy", "OK");
                return;
            }

            if (_fileService.GetAvailableClasses().Contains(className))
            {
                await DisplayAlert("B³¹d", "Klasa o tej nazwie ju¿ istnieje", "OK");
                return;
            }

            var newClass = new Class { ClassName = className };
            _fileService.SaveClass(newClass);
            NewClassNameEntry.Text = string.Empty;
            LoadClasses();

            await DisplayAlert("Sukces", $"Utworzono now¹ klasê: {className}", "OK");
        }

        private async void OnEditClassClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var classModel = (Class)button.BindingContext;

            await Navigation.PushAsync(new StudentListPage(classModel.ClassName));
        }

        private async void OnDeleteClassClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var classModel = (Class)button.BindingContext;

            bool answer = await DisplayAlert("Potwierdzenie",
                $"Czy na pewno chcesz usun¹æ klasê {classModel.ClassName}?", "Tak", "Nie");

            if (answer)
            {
                var filePath = Path.Combine(FileSystem.AppDataDirectory, $"{classModel.ClassName}.txt");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    LoadClasses();
                }
            }
        }
    }
}