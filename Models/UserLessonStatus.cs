namespace Finance_Literacy_App_Web.Models
{
    public class UserLessonStatus
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public string Status { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}