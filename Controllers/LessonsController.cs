using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Finance_Literacy_App_Web.Data;
using Finance_Literacy_App_Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Finance_Literacy_App_Web.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        private readonly Context _context;
        private readonly ILogger<LessonsController> _logger;

        public LessonsController(Context context, ILogger<LessonsController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        // GET: Lessons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt to Lessons/Details by unauthenticated user.");
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Lessons", new { id }) });
            }

            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);
            var lesson = await _context.Lessons
                .Include(l => l.Module)
                .Include(l => l.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            bool isLessonAvailable = false;
            if (!user.GroupId.HasValue)
            {
                var completedLessonIds = await _context.UserLessonStatuses
                    .Where(uls => uls.UserId == userId && uls.Status == "Completed")
                    .Select(uls => uls.LessonId)
                    .ToListAsync();

                var nextLesson = await _context.Lessons
                    .OrderBy(l => l.Id)
                    .FirstOrDefaultAsync(l => !completedLessonIds.Contains(l.Id));

                isLessonAvailable = nextLesson != null && nextLesson.Id == id;
            }
            else
            {
                var assignedLessonIds = await _context.GroupLessonDeadlines
                    .Where(gld => gld.GroupId == user.GroupId && gld.Deadline >= DateTime.Now.ToUniversalTime())
                    .Select(gld => gld.LessonId)
                    .ToListAsync();

                isLessonAvailable = assignedLessonIds.Contains(id.Value);
            }

            if (!isLessonAvailable)
            {
                return RedirectToAction("Index", "Home", new { error = "Lesson not available." });
            }

            _logger.LogInformation("Accessing Lessons/Details for user {UserName}. IsAuthenticated: {IsAuthenticated}.",
                User.Identity?.Name ?? "Anonymous",
                User.Identity?.IsAuthenticated ?? false);

            return View(lesson);
        }

        // POST: Lessons/SubmitAnswer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswer(Dictionary<int, string> answers, int[] taskIds)
        {
            if (answers == null || taskIds == null || !taskIds.Any())
            {
                return BadRequest("No answers provided.");
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var firstTask = await _context.Tasks.FindAsync(taskIds.First());
            if (firstTask == null)
            {
                return NotFound("Task not found.");
            }
            int lessonId = firstTask.LessonId;

            // Сохраняем все ответы пользователя
            foreach (var taskId in taskIds)
            {
                if (answers.TryGetValue(taskId, out var userAnswer))
                {
                    var userTaskAnswer = new UserTaskAnswer
                    {
                        UserId = userId,
                        TaskId = taskId,
                        Answer = userAnswer,
                        SubmittedAt = DateTime.Now.ToUniversalTime()
                    };
                    _context.UserTaskAnswers.Add(userTaskAnswer);
                }
            }

            // Отмечаем урок как выполненный независимо от правильности ответов
            var userLessonStatus = await _context.UserLessonStatuses
                .FirstOrDefaultAsync(uls => uls.UserId == userId && uls.LessonId == lessonId);

            if (userLessonStatus == null)
            {
                userLessonStatus = new UserLessonStatus
                {
                    UserId = userId,
                    LessonId = lessonId,
                    Status = "Completed",
                    CompletedAt = DateTime.Now.ToUniversalTime()
                };
                _context.UserLessonStatuses.Add(userLessonStatus);
            }
            else
            {
                userLessonStatus.Status = "Completed";
                userLessonStatus.CompletedAt = DateTime.Now.ToUniversalTime();
                _context.Entry(userLessonStatus).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("AnswerResults", new { lessonId });
        }

        public async Task<IActionResult> AnswerResults(int lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Module)
                .Include(l => l.Tasks)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userAnswers = await _context.UserTaskAnswers
                .Include(uta => uta.Task)
                .Where(uta => uta.UserId == userId && uta.Task.LessonId == lessonId)
                .ToListAsync();

            var answerResults = userAnswers.Select(uta => (
                TaskId: uta.TaskId,
                Question: uta.Task.Question,
                UserAnswer: uta.Answer,
                CorrectAnswer: uta.Task.CorrectAnswer,
                IsCorrect: string.Compare(uta.Answer?.Trim(), uta.Task.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase) == 0
            )).ToList();

            ViewBag.Lesson = lesson;
            return View(answerResults);
        }

        // GET: Lessons/Create
        public async Task<IActionResult> Create(int? moduleId)
        {
            System.Diagnostics.Debug.WriteLine($"Create GET - LessonId: {moduleId}");

            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "Title", moduleId);
            var lesson = new Lesson { ModuleId = moduleId ?? 0 };
            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lesson lesson, string[] taskQuestions, string[] taskAnswers)
        {
            System.Diagnostics.Debug.WriteLine($"Create POST - LessonId: {lesson.ModuleId}");

            ModelState.Remove("Module");

            if (ModelState.IsValid)
            {
                _context.Add(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageModules", "Admin");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine($"Validation Error: {error}");
            }

            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "Title", lesson.ModuleId);
            return View(lesson);
        }

        // GET: Lessons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
                return NotFound();

            ViewData["LessonId"] = new SelectList(_context.Modules, "Id", "Title", lesson.ModuleId);
            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lesson lesson, string[] taskQuestions, string[] taskAnswers)
        {
            System.Diagnostics.Debug.WriteLine($"Edit POST - ModuleId: {lesson.ModuleId}");

            if (id != lesson.Id)
                return NotFound();

            ModelState.Remove("Module");

            if (!_context.Lessons.Any(m => m.Id == lesson.ModuleId))
            {
                ModelState.AddModelError("ModuleId", "Выбранный модуль не существует.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lesson);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tasks.Any(e => e.Id == lesson.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction("ManageModules", "Admin");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine($"Validation Error: {error}");
            }

            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "Title", lesson.ModuleId);
            return View(lesson);
        }

        // GET: Lessons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.Module)
                .Include(l => l.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View("~/Views/Lessons/Delete.cshtml", lesson);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageModules", "Admin");
        }
    }
}