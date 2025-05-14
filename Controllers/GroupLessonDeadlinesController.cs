using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Finance_Literacy_App_Web.Data;
using Finance_Literacy_App_Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Finance_Literacy_App_Web.Controllers
{
    [Authorize(Roles = "teacher")]
    public class GroupLessonDeadlinesController : Controller
    {
        private readonly Context _context;

        public GroupLessonDeadlinesController(Context context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: GroupLessonDeadlines/Index
        public async Task<IActionResult> Index()
        {
            var deadlines = await _context.GroupLessonDeadlines
                .Include(gld => gld.Group)
                .Include(gld => gld.Lesson)
                .ToListAsync();
            return View(deadlines);
        }

        // GET: GroupLessonDeadlines/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Groups = new SelectList(await _context.Groups.ToListAsync(), "Id", "Name");
            ViewBag.Lessons = new SelectList(await _context.Lessons.ToListAsync(), "Id", "Title");
            return View();
        }

        // POST: GroupLessonDeadlines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupLessonDeadline groupLessonDeadline)
        {
            ModelState.Remove("Group");
            ModelState.Remove("Lesson");

            if (groupLessonDeadline.GroupId <= 0 || groupLessonDeadline.LessonId <= 0)
            {
                ModelState.AddModelError("", "Invalid group or lesson ID.");
            }

            // Проверка на существование дубликата
            var existingDeadline = await _context.GroupLessonDeadlines
                .FirstOrDefaultAsync(gld => gld.GroupId == groupLessonDeadline.GroupId && gld.LessonId == groupLessonDeadline.LessonId);
            if (existingDeadline != null)
            {
                ModelState.AddModelError("", "A deadline for this group and lesson already exists.");
            }

            if (ModelState.IsValid)
            {
                groupLessonDeadline.Deadline = groupLessonDeadline.Deadline.ToUniversalTime();
                _context.GroupLessonDeadlines.Add(groupLessonDeadline);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the deadline. It may already exist.");
                }
            }

            ViewBag.Groups = new SelectList(await _context.Groups.ToListAsync(), "Id", "Name", groupLessonDeadline.GroupId);
            ViewBag.Lessons = new SelectList(await _context.Lessons.ToListAsync(), "Id", "Title", groupLessonDeadline.LessonId);
            return View(groupLessonDeadline);
        }

        // GET: GroupLessonDeadlines/Edit
        public async Task<IActionResult> Edit(int? groupId, int? lessonId)
        {
            if (groupId == null || lessonId == null)
            {
                return NotFound();
            }

            var deadline = await _context.GroupLessonDeadlines
                .Include(gld => gld.Group)
                .Include(gld => gld.Lesson)
                .FirstOrDefaultAsync(gld => gld.GroupId == groupId && gld.LessonId == lessonId);
            if (deadline == null)
            {
                return NotFound();
            }

            ViewBag.Groups = new SelectList(await _context.Groups.ToListAsync(), "Id", "Name", deadline.GroupId);
            ViewBag.Lessons = new SelectList(await _context.Lessons.ToListAsync(), "Id", "Title", deadline.LessonId);
            return View(deadline);
        }

        // POST: GroupLessonDeadlines/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int groupId, int lessonId, GroupLessonDeadline groupLessonDeadline)
        {
            ModelState.Remove("Lesson");
            ModelState.Remove("Group");

            if (groupId != groupLessonDeadline.GroupId || lessonId != groupLessonDeadline.LessonId)
            {
                return NotFound();
            }

            if (groupLessonDeadline.GroupId <= 0 || groupLessonDeadline.LessonId <= 0)
            {
                ModelState.AddModelError("", "Invalid group or lesson ID.");
            }

            // Проверка на существование дубликата (кроме текущей записи)
            var existingDeadline = await _context.GroupLessonDeadlines
                .FirstOrDefaultAsync(gld => gld.GroupId == groupLessonDeadline.GroupId && gld.LessonId == groupLessonDeadline.LessonId && (gld.GroupId != groupId || gld.LessonId != lessonId));
            if (existingDeadline != null)
            {
                ModelState.AddModelError("", "A deadline for this group and lesson already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    groupLessonDeadline.Deadline = groupLessonDeadline.Deadline.ToUniversalTime();
                    _context.Update(groupLessonDeadline);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.GroupLessonDeadlines.Any(e => e.GroupId == groupLessonDeadline.GroupId && e.LessonId == groupLessonDeadline.LessonId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the deadline. It may already exist.");
                }
            }

            ViewBag.Groups = new SelectList(await _context.Groups.ToListAsync(), "Id", "Name", groupLessonDeadline.GroupId);
            ViewBag.Lessons = new SelectList(await _context.Lessons.ToListAsync(), "Id", "Title", groupLessonDeadline.LessonId);
            return View(groupLessonDeadline);
        }

        // GET: GroupLessonDeadlines/Delete
        public async Task<IActionResult> Delete(int? groupId, int? lessonId)
        {
            System.Diagnostics.Debug.WriteLine($"Delete GET - Received GroupId: {groupId}, LessonId: {lessonId}");

            if (groupId == null || lessonId == null)
            {
                return NotFound();
            }

            var deadline = await _context.GroupLessonDeadlines
                .Include(gld => gld.Group)
                .Include(gld => gld.Lesson)
                .FirstOrDefaultAsync(gld => gld.GroupId == groupId && gld.LessonId == lessonId);
            if (deadline == null)
            {
                System.Diagnostics.Debug.WriteLine($"Delete GET - Deadline not found for GroupId: {groupId}, LessonId: {lessonId}");
                return NotFound();
            }

            System.Diagnostics.Debug.WriteLine($"Delete GET - Found deadline: GroupId={deadline.GroupId}, LessonId={deadline.LessonId}");
            return View(deadline);
        }

        // POST: GroupLessonDeadlines/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int groupId, int lessonId)
        {
            System.Diagnostics.Debug.WriteLine($"DeleteConfirmed POST - Received GroupId: {groupId}, LessonId: {lessonId}");

            var deadline = await _context.GroupLessonDeadlines
                .FirstOrDefaultAsync(gld => gld.GroupId == groupId && gld.LessonId == lessonId);
            if (deadline == null)
            {
                return NotFound();
            }

            try
            {
                _context.GroupLessonDeadlines.Remove(deadline);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting deadline: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                TempData["ErrorMessage"] = "Cannot delete this deadline because it is referenced by other records.";
                return RedirectToAction("Index");
            }
        }
    }
}