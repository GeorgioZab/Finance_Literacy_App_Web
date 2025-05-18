using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Finance_Literacy_App_Web.Data;
using Finance_Literacy_App_Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using OfficeOpenXml;
using ClosedXML.Excel;

namespace Finance_Literacy_App_Web.Controllers
{
    [Authorize(Roles = "teacher")]
    public class TeacherController : Controller
    {
        private readonly Context _context;

        public TeacherController(Context context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Teacher/Groups
        public async Task<IActionResult> Groups()
        {
            var groups = await _context.Groups
                .Include(g => g.Users)
                .ToListAsync();
            return View(groups);
        }

        // GET: Teacher/CreateGroup
        public IActionResult CreateGroup()
        {
            return View();
        }

        // POST: Teacher/CreateGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                ModelState.AddModelError("", "Group name is required.");
                return View(groupName);
            }

            var group = new Group { Name = groupName, InviteCode = Guid.NewGuid().ToString("N").Substring(0, 8) };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return RedirectToAction("Groups");
        }

        // GET: Teacher/AssignLesson
        public IActionResult AssignLesson()
        {
            return RedirectToAction("Create", "GroupLessonDeadlines");
        }

        // GET: Teacher/DeleteGroup/5
        public async Task<IActionResult> DeleteGroup(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // POST: Teacher/DeleteGroup/5
        [HttpPost, ActionName("DeleteGroup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroupConfirmed(int id)
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            try
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
                return RedirectToAction("Groups");
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting group: {ex.Message}");
                TempData["ErrorMessage"] = "Cannot delete this group because it is referenced by other records.";
                return RedirectToAction("Groups");
            }
        }

        // GET: Teacher/RemoveUserFromGroup/5?userId=1
        public async Task<IActionResult> RemoveUserFromGroup(int id, string userId)
        {
            if (id == null || string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            var user = group.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.GroupId = id;
            ViewBag.UserId = userId;
            return View(user);
        }

        // POST: Teacher/RemoveUserFromGroup/5?userId=1
        [HttpPost, ActionName("RemoveUserFromGroup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromGroupConfirmed(int id, string userId)
        {
            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            var user = group.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                group.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Groups");
        }

        // GET: Teacher/ViewUserAnswers/5
        public async Task<IActionResult> ViewUserAnswers(int id)
        {
            var group = await _context.Groups
        .Include(g => g.Users)
        .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();

            var answers = await _context.UserTaskAnswers
                .Include(a => a.User)
                .Include(a => a.Task)
                .ThenInclude(t => t.Lesson)
                .ThenInclude(l => l.Module)
                .Where(a => a.User.GroupId == id)
                .ToListAsync();

            ViewBag.Group = group;
            ViewBag.Lessons = await _context.Lessons
                .Include(l => l.Module)
                .ToListAsync();

            return View(answers);
        }

        public async Task<IActionResult> ExportUserAnswersToExcel(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();

            var answers = await _context.UserTaskAnswers
                .Include(a => a.User)
                .Include(a => a.Task)
                .ThenInclude(t => t.Lesson)
                .ThenInclude(l => l.Module)
                .Where(a => a.User.GroupId == id)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("UserAnswers");
                worksheet.Cell(1, 1).Value = "User";
                worksheet.Cell(1, 2).Value = "Lesson";
                worksheet.Cell(1, 3).Value = "Task Question";
                worksheet.Cell(1, 4).Value = "User Answer";
                worksheet.Cell(1, 5).Value = "Correct Answer";
                worksheet.Cell(1, 6).Value = "Result";
                worksheet.Cell(1, 7).Value = "Submitted At";

                for (int i = 0; i < answers.Count; i++)
                {
                    var answer = answers[i];
                    var isCorrect = string.Compare(answer.Answer?.Trim(), answer.Task.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase) == 0;
                    worksheet.Cell(i + 2, 1).Value = answer.User.UserName;
                    worksheet.Cell(i + 2, 2).Value = $"{answer.Task.Lesson.Title} (Module: {answer.Task.Lesson.Module.Title})";
                    worksheet.Cell(i + 2, 3).Value = answer.Task.Question;
                    worksheet.Cell(i + 2, 4).Value = answer.Answer;
                    worksheet.Cell(i + 2, 5).Value = answer.Task.CorrectAnswer;
                    worksheet.Cell(i + 2, 6).Value = isCorrect ? "Correct" : "Incorrect";
                    worksheet.Cell(i + 2, 7).Value = answer.SubmittedAt.ToString("g");
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"UserAnswers_{group.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
            }
        }
    }
}
