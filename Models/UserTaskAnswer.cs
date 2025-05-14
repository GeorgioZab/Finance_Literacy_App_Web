namespace Finance_Literacy_App_Web.Models
{
    public class UserTaskAnswer
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int TaskId { get; set; }
        public string Answer { get; set; }
        public DateTime SubmittedAt { get; set; }

        public User User { get; set; }
        public Task Task { get; set; }
    }
}