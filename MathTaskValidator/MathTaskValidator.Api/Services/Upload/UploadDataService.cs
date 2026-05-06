using MathTaskValidator.Infrastructure;
using MathTaskValidator.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace MathTaskValidator.Api.Services
{
    public class UploadDataService : IUploadDataService
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;

        public UploadDataService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UploadDataAsync(IFormFile file)
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

                //Identify Teacher
                string? teacherUniqueId = GetAttributeValue(teacherElem, "ID", "Id", "id", "UniqueId");
                if (string.IsNullOrWhiteSpace(teacherUniqueId))
                    return false;

                // Load existing teacher with related students if present
                var teacher = await _context.Teachers
                    .Include(t => t.Students)
                    .ThenInclude(s => s.Exams)
                    .ThenInclude(e => e.Tasks)
                    .FirstOrDefaultAsync(t => t.UniqueId == teacherUniqueId);

                if (teacher is null)
                {
                    teacher = new Teacher(teacherUniqueId);
                    _context.Teachers.Add(teacher);
                }

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

                        student = new Student(studentUniqueId, teacher?.Id);
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
                            exam = new Exam(examUniqueId, student);
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

                            var task = new ExamTask { UniqueId = taskUniqueId, RawText = rawText, Exam = exam };
                            exam.Tasks.Add(task);
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string? GetAttributeValue(XElement elem, params string[] names)
        {
            foreach (var n in names)
            {
                var attr = elem.Attribute(n);
                if (attr != null && !string.IsNullOrWhiteSpace(attr.Value))
                    return attr.Value.Trim();
            }

            // try case-insensitive
            var found = elem.Attributes().FirstOrDefault(a => names.Any(n => string.Equals(a.Name.LocalName, n, StringComparison.OrdinalIgnoreCase)));
            return found?.Value?.Trim();
        }
    }
}
