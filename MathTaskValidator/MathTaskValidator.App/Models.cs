namespace MathTaskValidator.App
{
    public class UploadResponse
    {
        public bool Success { get; set; }
    }

    public class QueryResultsResponse
    {
        public List<ExamDto>? ExamResults { get; set; }
    }

    public class ExamDto
    {
        public string ExamUniqueId { get; set; } = string.Empty;
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
        public decimal Percentage { get; set; }
        public List<TaskResultDto>? TaskResults { get; set; }
    }

    public class TaskResultDto
    {
        public string TaskUniqueId { get; set; } = string.Empty;
        public decimal ExpectedValue { get; set; }
        public decimal StudentValue { get; set; }
        public bool IsCorrect { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class TeacherApiResponse
    {
        public List<StudentApiResult>? StudentsResults { get; set; }
    }

    public class StudentApiResult
    {
        public string StudentUniqueId { get; set; } = string.Empty;
        public List<ExamDto>? ExamResults { get; set; }
    }
}
