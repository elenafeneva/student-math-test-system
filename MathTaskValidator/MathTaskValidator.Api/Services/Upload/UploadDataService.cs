using MathTaskValidator.Core.Models;
using MathTaskValidator.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace MathTaskValidator.Api.Services
{
    public class UploadDataService : IUploadDataService
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UploadDataService> _logger;

        public UploadDataService(AppDbContext context, AppSettings appSettings, ILogger<UploadDataService> logger)
        {
            _context = context;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<bool> UploadDataAsync(IFormFile file, string? teacherUniqueId = null)
        {
            try
            {
                //Read from file stream and load XML document
                var doc = ParseXmlDocument(file);
                if (doc?.Root is null)
                    return false;

                // Determine teacher element (root is expected to be the teacher)
                var teacherElem = doc.Root;
                var xmlTeacherUniqueId = GetAttributeValue(teacherElem, "ID", "Id", "id", "UniqueId");

                if (string.IsNullOrWhiteSpace(xmlTeacherUniqueId) || xmlTeacherUniqueId != teacherUniqueId)
                    return false;

                var uploadBatchId = Guid.NewGuid().ToString();
                var teacher = await ResolveTeacherAsync(xmlTeacherUniqueId, uploadBatchId);

                // Process students for this teacher
                ProcessStudents(teacherElem, teacher, uploadBatchId);

                await using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }

                await SaveUploadedFileAsync(file, uploadBatchId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded file");
                return false;
            }
        }

        private static XDocument ParseXmlDocument(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            return XDocument.Load(stream);
        }

        private async Task<Teacher> ResolveTeacherAsync(string uniqueId, string uploadBatchId)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Students)
                .ThenInclude(s => s.Exams)
                .ThenInclude(e => e.Tasks)
                .FirstOrDefaultAsync(t => t.UniqueId == uniqueId);

            if (teacher is null)
            {
                teacher = new Teacher(uniqueId);
                _context.Teachers.Add(teacher);
            }

            teacher.ExternalId = uploadBatchId;
            return teacher;
        }

        private void ProcessStudents(XElement teacherElem, Teacher teacher, string uploadBatchId)
        {
            var studentElements = teacherElem.Descendants()
                .Where(e => string.Equals(e.Name.LocalName, "Student", StringComparison.OrdinalIgnoreCase));

            foreach (var studentElement in studentElements)
            {
                var studentUniqueId = GetAttributeValue(studentElement, "ID", "Id", "id", "UniqueId");
                if (string.IsNullOrWhiteSpace(studentUniqueId))
                    continue;

                var student = teacher.Students.FirstOrDefault(s => s.UniqueId == studentUniqueId);
                if (student is null)
                {
                    student = new Student(studentUniqueId, teacher.Id, uploadBatchId);
                    teacher.Students.Add(student);
                }

                ProcessExams(studentElement, student, uploadBatchId);
            }
        }

        private void ProcessExams(XElement studentElement, Student student, string uploadBatchId)
        {
            var examElements = studentElement.Descendants()
                .Where(e => string.Equals(e.Name.LocalName, "Exam", StringComparison.OrdinalIgnoreCase));

            foreach (var examElement in examElements)
            {
                var examUniqueId = GetAttributeValue(examElement, "ID", "Id", "id", "UniqueId");
                if (string.IsNullOrWhiteSpace(examUniqueId))
                    continue;

                var exam = student.Exams.FirstOrDefault(x => x.UniqueId == examUniqueId);
                if (exam is null)
                {
                    exam = new Exam(examUniqueId, student, uploadBatchId);
                    student.Exams.Add(exam);
                }

                ProcessTasks(examElement, exam, uploadBatchId);
            }
        }

        private static void ProcessTasks(XElement examElement, Exam exam, string uploadBatchId)
        {
            var taskElements = examElement.Descendants()
                .Where(t => string.Equals(t.Name.LocalName, "Task", StringComparison.OrdinalIgnoreCase));

            foreach (var taskElement in taskElements)
            {
                var taskUniqueId = GetAttributeValue(taskElement, "ID", "Id", "id", "UniqueId");
                if (string.IsNullOrWhiteSpace(taskUniqueId))
                    continue;

                var exists = exam.Tasks?.Any(t => t.UniqueId == taskUniqueId) ?? false;
                if (exists)
                    continue;

                var rawText = (taskElement.Value ?? string.Empty).Trim();
                var task = new ExamTask(taskUniqueId, rawText, uploadBatchId);
                exam.Tasks?.Add(task);
            }
        }

        private async Task SaveUploadedFileAsync(IFormFile file, string fileId)
        {
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), _appSettings.UploadsPath ?? "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(file.FileName);
            var uniqueName = fileId + ext;
            var savePath = Path.Combine(uploadsRoot, uniqueName);

            await using (var fs = File.Create(savePath))
            {
                await file.CopyToAsync(fs);
            }
        }

        private static string? GetAttributeValue(XElement elem, params string[] names)
        {
            foreach (var name in names)
            {
                var attr = elem.Attribute(name);
                if (attr != null && !string.IsNullOrWhiteSpace(attr.Value))
                    return attr.Value.Trim();
            }

            var found = elem.Attributes().FirstOrDefault(a => names.Any(n => string.Equals(a.Name.LocalName, n, StringComparison.OrdinalIgnoreCase)));
            return found?.Value?.Trim();
        }
    }
}
