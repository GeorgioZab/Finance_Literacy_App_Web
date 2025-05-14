using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Finance_Literacy_App_Web.Data;
using Finance_Literacy_App_Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace Finance_Literacy_App_Web.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly Context _context;

        public AdminController(Context context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Admin/ManageUsers
        public async Task<IActionResult> ManageUser()
        {
            System.Diagnostics.Debug.WriteLine("ManageUser action called");
            var users = await _context.Users.ToListAsync();
            return View("~/Views/Admin/ManageUser.cshtml", users);
        }

        // GET: Admin/ManageModules
        public async Task<IActionResult> ManageModules()
        {
            var modules = await _context.Modules
                .Include(m => m.Lessons)
                .ThenInclude(l => l.Tasks)
                .ToListAsync();
            return View(modules);
        }

        // GET: Admin/EditUserRole/5
        public async Task<IActionResult> EditUserRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = new List<string> { "admin", "teacher", "user" };
            ViewData["Roles"] = roles;

            return View("~/Views/Admin/EditUserRole.cshtml", user);
        }

        // POST: Admin/EditUserRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRole(string id, string role)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(role))
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Role = role;
            _context.Update(user);
            await _context.SaveChangesAsync();

            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, role);

            return RedirectToAction("ManageUser");
        }
    }
}