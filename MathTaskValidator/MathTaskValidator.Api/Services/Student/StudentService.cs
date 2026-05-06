using MathTaskValidator.Core.Models;
using MathTaskValidator.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MathTaskValidator.Api.Services
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Student> GetStudentByUniqueIdAsync(string studentUniqueId)
        {
            return await _context.Students
                .Include(s => s.Exams)
                .ThenInclude(e => e.Tasks)
                .FirstOrDefaultAsync(s => s.UniqueId == studentUniqueId) ?? new Student();
        }


        public async Task<List<Exam>> GetStudentExamsAsync(Guid studentId)
        {
            var studentExams = await _context.Exams
                .Where(e => e.StudentId == studentId)
                .ToListAsync();
            return studentExams;
        }

        public async Task<List<Student>> GetStudentsByTeacherUniqueIdAsync(string teacherUniqueId)
        {
            var teacherId = await _context.Teachers
                .Where(t => t.UniqueId == teacherUniqueId)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

            return await _context.Students
              .Include(s => s.Exams)
              .ThenInclude(e => e.Tasks)
              .Where(s => s.TeacherId == teacherId)
              .ToListAsync();
        }
    }
}
