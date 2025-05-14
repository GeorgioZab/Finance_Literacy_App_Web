namespace Finance_Literacy_App_Web.ViewModels
{
    public class TaskViewModel
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }

        public TaskViewModel() { }

        public TaskViewModel(Models.Task task)
        {
            Id = task.Id;
            Question = task.Question;
            CorrectAnswer = task.CorrectAnswer;
        }
    }
}