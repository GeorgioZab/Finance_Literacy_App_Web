using Finance_Literacy_App_Web.Data;
using Finance_Literacy_App_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Finance_Literacy_App_Web.Controllers
{
    public class TasksController : Controller
    {
        private readonly Context _context;

        public TasksController(Context context)
        {
            _context = context;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            var tasks = _context.Tasks.Include(t => t.Lesson).ThenInclude(l => l.Module);
            return View(await tasks.ToListAsync());
        }

        // GET: Tasks/Create
        public IActionResult Create(int? lessonId)
        {
            System.Diagnostics.Debug.WriteLine($"Create GET - LessonId: {lessonId}");

            if (!lessonId.HasValue || lessonId <= 0)
            {
                ModelState.AddModelError("", "Не указан урок для задания.");
            }
            else if (!_context.Lessons.Any(l => l.Id == lessonId))
            {
                ModelState.AddModelError("", "Выбранный урок не существует.");
            }

            ViewData["LessonId"] = new SelectList(_context.Lessons, "Id", "Title", lessonId);
            var task = new Models.Task { LessonId = lessonId ?? 0 };
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Question,CorrectAnswer,LessonId")] Models.Task task)
        {
            System.Diagnostics.Debug.WriteLine($"Create POST - LessonId: {task.LessonId}");

            ModelState.Remove("Lesson");

            if (!_context.Lessons.Any(l => l.Id == task.LessonId))
            {
                ModelState.AddModelError("LessonId", "Выбранный урок не существует.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine($"Validation Error: {error}");
            }

            ViewData["LessonId"] = new SelectList(_context.Lessons, "Id", "Title", task.LessonId);
            return View(task);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            ViewData["LessonId"] = new SelectList(_context.Lessons, "Id", "Title", task.LessonId);
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Question,CorrectAnswer,LessonId")] Models.Task task)
        {
            System.Diagnostics.Debug.WriteLine($"Edit POST - LessonId: {task.LessonId}");

            if (id != task.Id)
                return NotFound();

            ModelState.Remove("Lesson");

            if (!_context.Lessons.Any(l => l.Id == task.LessonId))
            {
                ModelState.AddModelError("LessonId", "Выбранный урок не существует.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tasks.Any(e => e.Id == task.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction("Index", "Home");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine($"Validation Error: {error}");
            }

            ViewData["LessonId"] = new SelectList(_context.Lessons, "Id", "Title", task.LessonId);
            return View(task);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var task = await _context.Tasks.Include(t => t.Lesson).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}