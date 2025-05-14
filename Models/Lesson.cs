using System.ComponentModel.DataAnnotations;

namespace Finance_Literacy_App_Web.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Необходимо выбрать модуль.")]
        public int ModuleId { get; set; }
        public Module Module { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<Task> Tasks { get; set; } = new List<Task>();
    }
}