using Finance_Literacy_App_Web.Models;

namespace Finance_Literacy_App_Web.ViewModels;

public class ModuleViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<LessonViewModel> Lessons { get; set; }

    public ModuleViewModel() { }

    public ModuleViewModel(Module module)
    {
        Id = module.Id;
        Title = module.Title;
        Description = module.Description;
        Lessons = module.Lessons?.Select(l => new LessonViewModel(l)).ToList() ?? new List<LessonViewModel>();
    }
}