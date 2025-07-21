using System.ComponentModel.DataAnnotations.Schema;

namespace TaskingSystem.Models
{
    public class lecture
    {
        public int lectureId { get; set; }
        public string lectureName { get; set; }
        public string ProfessorId { get; set; }
        public string CourseCode { get; set; }
        public string lectureURL { get; set; }
        [NotMapped]
        public IFormFile lectureFile { set; get; }

        // Navigation properties
        public Course? Course { get; set; }
        public ApplicationUser? Professor { get; set; }
    }
}
