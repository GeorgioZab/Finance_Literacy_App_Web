using System.ComponentModel.DataAnnotations;

namespace Finance_Literacy_App_Web.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Необходимо выбрать модуль.")]

        [Url(ErrorMessage = "Пожалуйста, введите корректную ссылку на YouTube-видео.")]
        [RegularExpression(@"^(https?\:\/\/)?(www\.youtube\.com|youtu\.be)\/.+$", ErrorMessage = "Ссылка должна быть на YouTube-видео.")]
        public string YouTubeVideoUrl { get; set; }
        public int ModuleId { get; set; }
        public Module Module { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<Task> Tasks { get; set; } = new List<Task>();
    }
}