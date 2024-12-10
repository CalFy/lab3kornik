using System.Collections.ObjectModel;
using System.Text.Json;

namespace lab3kornik;

public partial class MainPage : ContentPage
{
    public ObservableCollection<Student> Students { get; set; } = new();
    private readonly string _filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "students.json");

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadData();
    }

    private async void OnOpenFileClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Виберіть JSON-файл",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/json" } },
                    { DevicePlatform.WinUI, new[] { ".json" } },
                    { DevicePlatform.iOS, new[] { "json" } }
                })
            });

            if (result?.FullPath != null)
            {
                string json = File.ReadAllText(result.FullPath);
                var data = JsonSerializer.Deserialize<List<Student>>(json);
                if (data != null)
                {
                    Students.Clear();
                    foreach (var student in data)
                        Students.Add(student);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", ex.Message, "OK");
        }
    }

    private async void OnSaveFileClicked(object sender, EventArgs e)
    {
        try
        {
            string json = JsonSerializer.Serialize(Students.ToList());
            File.WriteAllText(_filePath, json);
            await DisplayAlert("Успіх", "Файл збережено!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", ex.Message, "OK");
        }
    }

    private void LoadData()
    {
        if (File.Exists(_filePath))
        {
            string json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<List<Student>>(json);
            if (data != null)
            {
                Students.Clear();
                foreach (var student in data)
                    Students.Add(student);
            }
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        var newStudent = await StudentFormPage.Show(null);
        if (newStudent != null)
        {
            // Генеруємо унікальний ID, якщо необхідно
            if (Students.Any(s => s.Id == newStudent.Id))
            {
                newStudent.Id = Students.Max(s => s.Id) + 1;
            }

            Students.Add(newStudent);
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (DataList.SelectedItem is Student selectedStudent)
        {
            var editedStudent = await StudentFormPage.Show(selectedStudent);
            if (editedStudent != null)
            {
                int index = Students.IndexOf(selectedStudent);
                Students[index] = editedStudent;
            }
        }
        else
        {
            await DisplayAlert("Увага", "Виберіть студента для редагування!", "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (DataList.SelectedItem is Student selectedStudent)
        {
            Students.Remove(selectedStudent);
        }
        else
        {
            await DisplayAlert("Увага", "Виберіть студента для видалення!", "OK");
        }
    }
}
