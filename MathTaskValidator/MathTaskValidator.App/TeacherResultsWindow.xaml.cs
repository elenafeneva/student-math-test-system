using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace MathTaskValidator.App
{
    public partial class TeacherResultsWindow : Window
    {
        public TeacherResultsWindow()
        {
            InitializeComponent();
        }

        public TeacherResultsWindow(string teacherId) : this()
        {
            if (!string.IsNullOrWhiteSpace(teacherId))
                TeacherIdInput.Text = teacherId;
        }

        private class StudentResultDto
        {
            public string StudentUniqueId { get; set; } = string.Empty;
            public string Summary { get; set; } = string.Empty;
        }

        private async void LoadStudents_Click(object sender, RoutedEventArgs e)
        {
            var teacherId = TeacherIdInput?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                MessageBox.Show("Enter teacher unique id");
                return;
            }

            try
            {
                using var client = new HttpClient();
                var url = $"https://localhost:44376/api/teachers/{System.Uri.EscapeDataString(teacherId)}/students/results";
                var resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Failed to load: {resp.StatusCode}");
                    return;
                }

                var api = await resp.Content.ReadFromJsonAsync<TeacherApiResponse>();
                StudentsList.Items.Clear();
                foreach (var s in api.StudentsResults)
                {
                    var first = s.ExamResults.FirstOrDefault();
                    var summary = first == null ? "No exams" : $"{first.CorrectCount}/{first.TotalCount} ({first.Percentage:F0}%)";
                    StudentsList.Items.Add(new StudentResultDto { StudentUniqueId = s.StudentUniqueId, Summary = summary });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}