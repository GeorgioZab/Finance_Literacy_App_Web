using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Finance_Literacy_App_Web.Data;
using Finance_Literacy_App_Web.Models;
using Microsoft.EntityFrameworkCore;
using System;

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
            if (group == null)
            {
                return NotFound();
            }

            var userIds = group.Users.Select(u => u.Id).ToList();
            var userAnswers = await _context.UserTaskAnswers
                .Include(uta => uta.User)
                .Include(uta => uta.Task)
                .ThenInclude(t => t.Lesson)
                .ThenInclude(l => l.Module)
                .Where(uta => userIds.Contains(uta.UserId))
                .ToListAsync();

            ViewBag.Group = group;
            return View(userAnswers);
        }
    }
}