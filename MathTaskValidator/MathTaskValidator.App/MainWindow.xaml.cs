using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;

namespace MathTaskValidator.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private class UploadResponse
        {
            public bool Success { get; set; }
        }

        private class QueryResultsResponse
        {
            public List<ExamDto>? ExamResults { get; set; }
        }

        private class ExamDto
        {
            public string ExamUniqueId { get; set; } = string.Empty;
            public int CorrectCount { get; set; }
            public int TotalCount { get; set; }
            public decimal Percentage { get; set; }
            public List<TaskResultDto>? TaskResults { get; set; }
        }

        private class TaskResultDto
        {
            public string TaskUniqueId { get; set; } = string.Empty;
            public decimal ExpectedValue { get; set; }
            public decimal StudentValue { get; set; }
            public bool IsCorrect { get; set; }
            public string? ErrorMessage { get; set; }
        }

        public async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                // Changed filter to XML to match your project requirements
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    AttachedFilePath.Text = Path.GetFileName(filePath);

                    using var client = new HttpClient();

                    // Prepare the file stream and content. Keep the stream alive until the request completes.
                    using var fileStream = File.OpenRead(filePath);
                    using var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");                    

                    using var content = new MultipartFormDataContent();

                    // "File" must match the property name in your UploadCommand.Request
                    content.Add(fileContent, "File", Path.GetFileName(filePath));

                    // If a teacher unique id was entered in the UI, include it in the multipart form
                    var teacherId = TeacherIdBox?.Text?.Trim();
                    if (!string.IsNullOrWhiteSpace(teacherId))
                    {
                        content.Add(new StringContent(teacherId), "TeacherUniqueId");
                    }

                    // Use configured API base URL
                    var uploadUrl = AppSettings.GetApiBaseUrl().TrimEnd('/') + "/api/upload";
                    var response = await client.PostAsync(uploadUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await response.Content.ReadFromJsonAsync<UploadResponse>();
                        if (apiResponse?.Success == true)
                        {
                            MessageBox.Show("File successfully uploaded to the database!");
                        }
                        else
                        {
                            MessageBox.Show("Upload failed: server returned unsuccessful result.");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Upload failed: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }

        private void TeacherMode_Click(object sender, RoutedEventArgs e)
        {
            RoleSelectionBox.Visibility = Visibility.Collapsed;
            TeacherView.Visibility = Visibility.Visible;
        }

        private async void StudentIdBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            var studentId = StudentIdBox?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(studentId))
            {
                MessageBox.Show("Please enter a student unique id.");
                return;
            }

            try
            {
                using var client = new HttpClient();
                var url = AppSettings.GetApiBaseUrl().TrimEnd('/') + $"/api/students/{Uri.EscapeDataString(studentId)}/results";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Failed to get results: {response.StatusCode}");
                    return;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<QueryResultsResponse>();
                ExamList.Items.Clear();
                if (apiResponse?.ExamResults is null || apiResponse.ExamResults.Count == 0)
                {
                    ExamList.Items.Add("No exams found for this student.");
                    return;
                }

                foreach (var examResult in apiResponse.ExamResults)
                {
                    var header = string.IsNullOrWhiteSpace(examResult.ExamUniqueId)
                        ? $"Exam: {examResult.CorrectCount}/{examResult.TotalCount} ({examResult.Percentage:F1}%)"
                        : $"Exam: {examResult.ExamUniqueId} - {examResult.CorrectCount}/{examResult.TotalCount} ({examResult.Percentage:F1}%)";
                    ExamList.Items.Add(header);
                    // List detailed task results under each exam
                    if (examResult.TaskResults != null)
                    {
                        foreach (var taskResult in examResult.TaskResults)
                        {
                            var prefix = taskResult.IsCorrect ? "  ✓" : "  ✗"; // show check or cross
                            ExamList.Items.Add($"{prefix} Task {taskResult.TaskUniqueId}: student={taskResult.StudentValue} expected={taskResult.ExpectedValue} - {taskResult.ErrorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching results: {ex.Message}");
            }
        }

        private void StudentMode_Click(object sender, RoutedEventArgs e)
        {
            RoleSelectionBox.Visibility = Visibility.Collapsed;
            StudentView.Visibility = Visibility.Visible;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Reset to the main selection screen
            TeacherView.Visibility = Visibility.Collapsed;
            StudentView.Visibility = Visibility.Collapsed;
            RoleSelectionBox.Visibility = Visibility.Visible;
        }

        private void OpenTeacherResultsWindow_Click(object sender, RoutedEventArgs e)
        {
            // Open the teacher results window and prefill teacher id from the textbox
            var teacherId = TeacherIdBox?.Text?.Trim();
            var win = new TeacherResultsWindow(teacherId);
            win.Owner = this;
            win.ShowDialog();
        }
    }
}