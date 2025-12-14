using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;
using StoreManagementBlazor.ViewModels;
using System.Security.Claims;

namespace StoreManagementBlazor.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> LoginAsync(LoginViewModel model)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                return false;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("FullName", user.FullName ?? "")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            return true;
        }

        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext!
                .SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public ClaimsPrincipal? GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User;
        }
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.FullName))
                return (false, "Vui lòng nhập đầy đủ thông tin.");
            if (model.Password != model.ConfirmPassword)
                return (false, "Mật khẩu xác nhận không khớp.");

            var existedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (existedUser != null)
                return (false, "Tên đăng nhập đã tồn tại.");

            var user = new User
            {
                Username = model.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                FullName = model.FullName,
                Role = "Customer",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            // Lấy Id vừa tạo
            var customer = new Customer
            {
                UserId = user.UserId,
                Name = model.FullName,
                CreatedAt = DateTime.Now
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return (true, "Đăng ký thành công! Bạn có thể đăng nhập.");
        }
    }
}
