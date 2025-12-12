using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;
using BCrypt.Net;

namespace StoreManagementBlazor.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetUsersAsync(string? keyword, string? role)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(u => u.Username.Contains(keyword) || (u.FullName != null && u.FullName.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(role) && role != "all")
            {
                query = query.Where(u => u.Role == role);
            }

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<(bool success, string message)> CreateUserAsync(User user, string plainPassword)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                return (false, "Tên đăng nhập đã tồn tại.");
            }

            // Mặc định role là staff nếu chưa chọn
            if (string.IsNullOrEmpty(user.Role)) user.Role = "staff";
            
            user.CreatedAt = DateTime.Now;

            string passToHash = string.IsNullOrEmpty(plainPassword) ? "123456" : plainPassword;
            user.Password = BCrypt.Net.BCrypt.HashPassword(passToHash);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return (true, "Tạo người dùng thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateUserAsync(User user, string? newPassword)
        {
            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null) return (false, "Người dùng không tồn tại.");

            existingUser.FullName = user.FullName;
            existingUser.Role = user.Role;

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            try
            {
                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return (false, "Không tìm thấy người dùng.");

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return (true, "Đã xóa người dùng.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi xóa: {ex.Message}");
            }
        }
    }
}