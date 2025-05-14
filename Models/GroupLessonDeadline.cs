using System.ComponentModel.DataAnnotations;

namespace Finance_Literacy_App_Web.Models
{
    public class GroupLessonDeadline
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Необходимо выбрать группу.")]
        public int GroupId { get; set; }
        public Group Group { get; set; }
        [Required(ErrorMessage = "Необходимо выбрать урок.")]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public DateTime Deadline { get; set; }
    }
}