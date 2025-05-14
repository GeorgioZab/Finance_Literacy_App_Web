using Finance_Literacy_App_Web.Data;
using Finance_Literacy_App_Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Finance_Literacy_App_Web.Services
{
    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly Context _context;

        public AuthService(UserManager<User> userManager, Context context)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Context GetContext()
        {
            return _context;
        }

        public async Task<User> Register(string username, string email, string password, string role = "user")
        {
            if (await _userManager.FindByNameAsync(username) != null)
                throw new Exception("Такой пользователь уже зарегистрирован");

            if (await _userManager.FindByEmailAsync(email) != null)
                throw new Exception("Такой адрес уже зарегистрирован");

            if (password.Length < 6 || !password.Any(char.IsDigit))
                throw new Exception("Пароль должен содержать не менее 6 символов и цифру");

            var user = new User
            {
                UserName = username,
                Email = email,
                Role = role,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                return user;
            }
            throw new Exception("Регистрация не удалась: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<User> Authenticate(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null || !user.IsActive)
                return null;

            var isValid = await _userManager.CheckPasswordAsync(user, password);
            return isValid ? user : null;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}