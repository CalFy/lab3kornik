using System;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace lab3kornik;

public partial class StudentFormPage : ContentPage
{
    private Student _student;

    public StudentFormPage(Student? student)
    {
        InitializeComponent();
        _student = student ?? new Student();

        // Ініціалізація значень на формі
        NameEntry.Text = _student.Name;
        GroupEntry.Text = _student.Group;
        YearEntry.Text = _student.Year.ToString();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Перевірка на заповненість полів
        if (string.IsNullOrWhiteSpace(NameEntry.Text) || string.IsNullOrWhiteSpace(GroupEntry.Text) ||
            !int.TryParse(YearEntry.Text, out int year))
        {
            await DisplayAlert("Помилка", "Всі поля мають бути заповнені!", "ОК");
            return;
        }

        // Генеруємо унікальний ID, якщо це новий студент
        if (_student.Id == 0)
        {
            _student.Id = new Random().Next(1, int.MaxValue);
        }

        // Оновлення даних студента
        _student.Name = NameEntry.Text;
        _student.Group = GroupEntry.Text;
        _student.Year = year;

        // Відправка повідомлення через WeakReferenceMessenger
        WeakReferenceMessenger.Default.Send(_student);

        // Закриття сторінки після збереження
        await Navigation.PopAsync();
    }

    public static async Task<Student?> Show(Student? student)
    {
        var tcs = new TaskCompletionSource<Student?>();

        // Створення сторінки та реєстрація Messenger
        var page = new StudentFormPage(student);
        var messenger = WeakReferenceMessenger.Default;

        messenger.Register<Student>(page, (r, s) =>
        {
            // Відправлення результату після збереження
            messenger.Unregister<Student>(r);
            tcs.SetResult(s);
        });

        // Навігація на сторінку
        await Application.Current.MainPage.Navigation.PushAsync(page);
        return await tcs.Task;
    }
}
