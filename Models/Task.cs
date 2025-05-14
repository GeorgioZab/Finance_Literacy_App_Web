using System.ComponentModel.DataAnnotations;

namespace Finance_Literacy_App_Web.Models
{
    public class Task
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Необходимо выбрать урок.")]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
    }
}