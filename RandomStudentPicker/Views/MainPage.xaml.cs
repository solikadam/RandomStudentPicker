using RandomStudentPicker.Models;
using RandomStudentPicker.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RandomStudentPicker.Views
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<string> Classes { get; private set; } = new();

        public MainPage()
        {
            InitializeComponent();
            LoadClasses();
        }

        private async void LoadClasses()
        {
            var classes = await FileService.GetClassesAsync();
            Classes.Clear();
            foreach (var className in classes)
            {
                Classes.Add(className);
            }
            ClassesList.ItemsSource = Classes;
        }

        private async void OnAddClassClicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Nowa Klasa", "Podaj nazwę klasy:");
            if (!string.IsNullOrEmpty(result))
            {
                await FileService.AddClassAsync(result);
                LoadClasses();
            }
        }

        private async void OnDeleteClassClicked(object sender, EventArgs e)
        {
            if (!Classes.Any())
            {
                await DisplayAlert("Błąd", "Brak dostępnych klas do usunięcia.", "OK");
                return;
            }

            string selectedClass = await DisplayActionSheet("Wybierz klasę do usunięcia", "Anuluj", null, Classes.ToArray());

            if (!string.IsNullOrEmpty(selectedClass) && selectedClass != "Anuluj")
            {
                bool confirm = await DisplayAlert("Usuń Klasę", $"Czy na pewno chcesz usunąć klasę {selectedClass}?", "Tak", "Nie");
                if (confirm)
                {
                    await FileService.DeleteClassAsync(selectedClass);
                    LoadClasses();
                }
            }
        }

        private void OnClassSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            string selectedClass = e.SelectedItem.ToString();
            Navigation.PushAsync(new DetailClassPage(selectedClass));

            ((ListView)sender).SelectedItem = null;
        }
    }
}