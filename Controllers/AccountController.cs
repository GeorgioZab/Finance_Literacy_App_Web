using Microsoft.AspNetCore.Mvc;
using Finance_Literacy_App_Web.Models;
using Finance_Literacy_App_Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Finance_Literacy_App_Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: Account/JoinGroup
        [Authorize]
        public IActionResult JoinGroup()
        {
            return View();
        }

        // POST: Account/JoinGroup
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinGroup(string inviteCode)
        {
            if (string.IsNullOrEmpty(inviteCode))
            {
                ModelState.AddModelError("", "Invite code is required.");
                return View();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID is null or empty.");
                return RedirectToAction("Index", "Home");
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return RedirectToAction("Index", "Home");
            }

            var context = _authService.GetContext();
            if (context == null)
            {
                _logger.LogError("Context is null in AuthService.");
                return StatusCode(500);
            }

            var group = await context.Groups.FirstOrDefaultAsync(g => g.InviteCode == inviteCode);
            if (group == null)
            {
                ModelState.AddModelError("", "Invalid invite code.");
                return View();
            }

            if (user.GroupId.HasValue)
            {
                ModelState.AddModelError("", "You are already a member of a group.");
                return View();
            }

            user.GroupId = group.Id;
            await context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var user = await _authService.Authenticate(username, password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal);
            return LocalRedirect(returnUrl ?? "/");
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string role = "user")
        {
            try
            {
                var user = await _authService.Register(username, email, password, role);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }
    }
}