using Finance_Literacy_App_Web.Models;

namespace Finance_Literacy_App_Web.ViewModels
{
    public class LessonViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<TaskViewModel> Tasks { get; set; }

        public LessonViewModel() { }

        public LessonViewModel(Lesson lesson)
        {
            Id = lesson.Id;
            Title = lesson.Title;
            Content = lesson.Content;
            Tasks = lesson.Tasks?.Select(t => new TaskViewModel(t)).ToList() ?? new List<TaskViewModel>();
        }
    }
}