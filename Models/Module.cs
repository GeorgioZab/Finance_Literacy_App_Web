namespace Finance_Literacy_App_Web.Models
{
    public class Module
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
