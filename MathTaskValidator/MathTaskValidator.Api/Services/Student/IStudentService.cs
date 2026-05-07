using MathTaskValidator.Core.Models;

namespace MathTaskValidator.Api.Services
{
    public interface IStudentService
    {
        Task<Student?> GetStudentByUniqueIdAsync(string studentUniqueId);
        Task<List<Student>> GetStudentsByTeacherUniqueIdAsync(string teacherUniqueId);
    }
}
