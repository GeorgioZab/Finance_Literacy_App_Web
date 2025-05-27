using Microsoft.AspNetCore.Mvc;
using Finance_Literacy_App_Web.ViewModelBuilders;
using Microsoft.AspNetCore.Authorization;
using Finance_Literacy_App_Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Finance_Literacy_App_Web.Models;
using System.Security.Claims;

namespace Finance_Literacy_App_Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Context _context;

        public HomeController(ILogger<HomeController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users
                    .Include(u => u.Group)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (User.IsInRole("user"))
                {
                    var lessons = new List<Lesson>();
                    var userLessonStatuses = await _context.UserLessonStatuses
                        .Where(uls => uls.UserId == userId)
                        .ToListAsync();

                    if (user?.GroupId.HasValue == true) // Пользователь в группе
                    {
                        var assignedLessonIds = await _context.GroupLessonDeadlines
                            .Where(gld => gld.GroupId == user.GroupId)
                            .Select(gld => gld.LessonId)
                            .ToListAsync();

                        lessons = await _context.Lessons
                            .Include(l => l.Module)
                            .Where(l => assignedLessonIds.Contains(l.Id))
                            .OrderBy(l => l.ModuleId)
                            .ThenBy(l => l.Id)
                            .ToListAsync();
                    }
                    else // Пользователь без группы
                    {
                        lessons = await _context.Lessons
                            .Include(l => l.Module)
                            .OrderBy(l => l.ModuleId)
                            .ThenBy(l => l.Id)
                            .ToListAsync();
                    }

                    // Определяем первый незавершенный урок внутри каждого модуля
                    var groupedLessons = lessons.GroupBy(l => l.ModuleId);
                    var firstIncompleteLesson = null as Lesson;
                    foreach (var group in groupedLessons)
                    {
                        var incomplete = group
                            .OrderBy(l => l.Id)
                            .FirstOrDefault(l => !userLessonStatuses.Any(uls => uls.LessonId == l.Id && uls.Status == "Completed"));
                        if (incomplete != null)
                        {
                            firstIncompleteLesson = incomplete;
                            break;
                        }
                    }

                    var groupLessonDeadlines = user?.GroupId.HasValue == true
                        ? await _context.GroupLessonDeadlines
                            .Where(gld => gld.GroupId == user.GroupId)
                            .ToListAsync()
                        : new List<GroupLessonDeadline>();

                    ViewData["AvailableLessons"] = lessons;
                    ViewData["UserLessonStatuses"] = userLessonStatuses;
                    ViewData["FirstIncompleteLessonId"] = firstIncompleteLesson?.Id;
                    ViewData["IsInGroup"] = user?.GroupId.HasValue == true;
                    ViewData["GroupLessonDeadlines"] = groupLessonDeadlines;
                    ViewData["UserGroupName"] = user?.Group?.Name;
                }
                else if (User.IsInRole("admin"))
                {
                    // Для админа загружаем все уроки, модули и группы
                    var lessons = await _context.Lessons
                        .Include(l => l.Module)
                        .OrderBy(l => l.ModuleId)
                        .ThenBy(l => l.Id)
                        .ToListAsync();

                    var modules = await _context.Modules
                        .OrderBy(m => m.Id)
                        .ToListAsync();

                    var groups = await _context.Groups
                        .Include(g => g.Users)
                        .OrderBy(g => g.Name)
                        .ToListAsync();

                    ViewData["Lessons"] = lessons;
                    ViewData["Modules"] = modules;
                    ViewData["Groups"] = groups;
                }
                else if (User.IsInRole("teacher"))
                {
                    // Для учителя загружаем группы, которые он ведет, и уроки
                    var teacherGroups = await _context.Groups
                        .Include(g => g.Users)
                        .OrderBy(g => g.Name)
                        .ToListAsync();

                    var lessons = await _context.Lessons
                        .Include(l => l.Module)
                        .OrderBy(l => l.ModuleId)
                        .ThenBy(l => l.Id)
                        .ToListAsync();

                    var groupLessonDeadlines = await _context.GroupLessonDeadlines
                        .Include(gld => gld.Group)
                        .Include(gld => gld.Lesson)
                        .ToListAsync();

                    ViewData["TeacherGroups"] = teacherGroups;
                    ViewData["Lessons"] = lessons;
                    ViewData["GroupLessonDeadlines"] = groupLessonDeadlines;
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}