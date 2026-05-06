using MathTaskValidator.Core.Models;
using MathTaskValidator.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;

namespace MathTaskValidator.Api.Services
{
    public class UploadDataService : IUploadDataService
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;

        public UploadDataService(AppDbContext context, AppSettings appSettings)
        {
            _context = context;
            _appSettings = appSettings;
        }

        public async Task<bool> UploadDataAsync(IFormFile file, string? teacherUniqueId = null)
        {
            try
            {
                //Read from file stream and load XML document
                XDocument doc;
                using (var stream = file.OpenReadStream())
                {
                    doc = XDocument.Load(stream);
                }

                var root = doc.Root;
                if (root == null)
                    return false;

                // Determine teacher element (root is expected to be the teacher)
                var teacherElem = root;

                //Identify Teacher. Prefer provided teacherUniqueId, otherwise read from XML.
                string? xmlTeacherUniqueId = GetAttributeValue(teacherElem, "ID", "Id", "id", "UniqueId");
                if (xmlTeacherUniqueId != teacherUniqueId)
                    return false; // Mismatch between provided teacherUniqueId and XML;

                if (string.IsNullOrWhiteSpace(xmlTeacherUniqueId))
                    return false;

                // Load existing teacher with related students if present
                var teacher = await _context.Teachers
                    .Include(t => t.Students)
                    .ThenInclude(s => s.Exams)
                    .ThenInclude(e => e.Tasks)
                    .FirstOrDefaultAsync(t => t.UniqueId == xmlTeacherUniqueId);

                if (teacher is null)
                {
                    teacher = new Teacher(xmlTeacherUniqueId);
                    _context.Teachers.Add(teacher);
                }

                // Save uploaded file and store generated id in the teacher's ExternalId
                var fileId = Guid.NewGuid().ToString();
                teacher.ExternalId = fileId;

                // Process students for this teacher
                var studentElements = teacherElem.Descendants().Where(e => string.Equals(e.Name.LocalName, "Student", StringComparison.OrdinalIgnoreCase));
                foreach (var studentElement in studentElements)
                {
                    var studentUniqueId = GetAttributeValue(studentElement, "ID", "Id", "id", "UniqueId");
                    if (string.IsNullOrWhiteSpace(studentUniqueId))
                        continue;

                    var student = teacher?.Students?.FirstOrDefault(s => s.UniqueId == studentUniqueId);
                    if (student is null)
                    {
                        student = new Student(studentUniqueId, teacher?.Id, fileId);
                        teacher?.Students.Add(student);
                    }


                    // Process exams
                    var examElements = studentElement.Descendants().Where(e => string.Equals(e.Name.LocalName, "Exam", StringComparison.OrdinalIgnoreCase));
                    foreach (var examElement in examElements)
                    {
                        var examUniqueId = GetAttributeValue(examElement, "ID", "Id", "id", "UniqueId");
                        if (string.IsNullOrWhiteSpace(examUniqueId))
                            continue;

                        var exam = student.Exams.FirstOrDefault(x => x.UniqueId == examUniqueId);
                        if (exam is null)
                        {
                            exam = new Exam(examUniqueId, student, fileId);
                            student.Exams.Add(exam);
                        }

                        // Process tasks
                        var taskElements = examElement.Descendants().Where(t => string.Equals(t.Name.LocalName, "Task", StringComparison.OrdinalIgnoreCase));
                        foreach (var taskElement in taskElements)
                        {
                            var taskUniqueId = GetAttributeValue(taskElement, "ID", "Id", "id", "UniqueId");
                            var rawText = (taskElement.Value ?? string.Empty).Trim();
                            if (string.IsNullOrWhiteSpace(taskUniqueId))
                                continue;

                            // Check for duplicates
                            var exists = exam.Tasks?.Any(t => t.UniqueId == taskUniqueId) ?? false;
                            if (exists)
                                continue;

                            var task = new ExamTask(taskUniqueId, rawText, fileId);
                            exam.Tasks.Add(task);
                        }
                    }
                }

                // Save everything in a transaction
                await using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    SaveUploadedFileAsync(file, fileId);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing uploaded file: {ex.Message}");
                return false;
            }
        }

        private async void SaveUploadedFileAsync(IFormFile file, string fileId)
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

            // try case-insensitive
            var found = elem.Attributes().FirstOrDefault(a => names.Any(n => string.Equals(a.Name.LocalName, n, StringComparison.OrdinalIgnoreCase)));
            return found?.Value?.Trim();
        }
    }
}
