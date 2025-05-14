using Microsoft.AspNetCore.Mvc;
using Finance_Literacy_App_Web.ViewModelBuilders;
using Microsoft.AspNetCore.Authorization;
using Finance_Literacy_App_Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Finance_Literacy_App_Web.Models;

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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);

            System.Diagnostics.Debug.WriteLine($"Index - User GroupId: {user.GroupId}");

            var lessons = await _context.Lessons
                .Include(l => l.Module)
                .Include(l => l.Tasks)
                .OrderBy(l => l.Id)
                .ToListAsync();

            var userLessonStatuses = await _context.UserLessonStatuses
                .Where(uls => uls.UserId == userId)
                .ToListAsync();

            System.Diagnostics.Debug.WriteLine($"Index - Loaded UserLessonStatuses for UserId {userId}:");
            foreach (var uls in userLessonStatuses)
            {
                System.Diagnostics.Debug.WriteLine($"LessonId={uls.LessonId}, Status={uls.Status}, CompletedAt={uls.CompletedAt}");
            }

            var groupLessonDeadlines = user.GroupId.HasValue
                ? await _context.GroupLessonDeadlines
                    .Where(gld => gld.GroupId == user.GroupId)
                    .ToListAsync()
                : new List<GroupLessonDeadline>();

            System.Diagnostics.Debug.WriteLine($"Index - Loaded GroupLessonDeadlines for GroupId {user.GroupId}:");
            foreach (var gld in groupLessonDeadlines)
            {
                System.Diagnostics.Debug.WriteLine($"LessonId={gld.LessonId}, GroupId={gld.GroupId}, Deadline={gld.Deadline}");
            }

            var availableLessons = new List<Finance_Literacy_App_Web.Models.Lesson>();
            if (!user.GroupId.HasValue) // Самостоятельное обучение
            {
                availableLessons = lessons;
            }
            else // Обучение с учителем
            {
                var assignedLessonIds = groupLessonDeadlines
                    .Where(gld => gld.GroupId == user.GroupId)
                    .Select(gld => gld.LessonId)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Index - Assigned Lesson IDs: {string.Join(", ", assignedLessonIds)}");

                availableLessons = lessons
                    .Where(l => assignedLessonIds.Contains(l.Id))
                    .ToList();
            }

            System.Diagnostics.Debug.WriteLine($"Index - Available Lessons Count: {availableLessons.Count}");

            ViewData["AvailableLessons"] = availableLessons;
            ViewData["UserLessonStatuses"] = userLessonStatuses;
            ViewData["IsInGroup"] = user.GroupId.HasValue;
            ViewData["GroupLessonDeadlines"] = groupLessonDeadlines; // Передаем дедлайны

            if (!user.GroupId.HasValue)
            {
                var completedLessonIds = userLessonStatuses
                    .Where(uls => uls.Status == "Completed")
                    .Select(uls => uls.LessonId)
                    .ToList();

                var firstIncompleteLesson = lessons
                    .OrderBy(l => l.Id)
                    .FirstOrDefault(l => !completedLessonIds.Contains(l.Id));

                ViewData["FirstIncompleteLessonId"] = firstIncompleteLesson?.Id;
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