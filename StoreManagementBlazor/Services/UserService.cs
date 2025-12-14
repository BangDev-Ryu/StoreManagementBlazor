using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;
using StoreManagementBlazor.Models.ViewModels;

namespace StoreManagementBlazor.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================================================================
        // PHẦN 1: DÀNH CHO TRANG "THÔNG TIN CÁ NHÂN" (MyProfile.razor)
        // ========================================================================

        public async Task<UserProfileViewModel?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return new UserProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName ?? "",
                CreatedAt = user.CreatedAt,
                
                // --- QUAN TRỌNG: Phải gán Role để giao diện biết đường xử lý ---
                Role = user.Role ?? "staff", 
                
                // Lấy thông tin từ Customer nếu có
                Phone = customer?.Phone,
                Email = customer?.Email,
                Address = customer?.Address
            };
        }

        public async Task<(bool success, string message)> UpdateProfileAsync(UserProfileViewModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null) return (false, "Tài khoản không tồn tại.");

            // 1. Cập nhật bảng User
            // Chỉ cập nhật FullName nếu là Customer (theo yêu cầu hiển thị của bạn)
            // Tuy nhiên, thường thì User table vẫn giữ FullName, nên ta cứ update.
            // Nhưng nếu bạn muốn admin không sửa tên ở đây thì có thể bọc if.
            // Ở đây mình vẫn cho lưu FullName vào bảng User để đồng bộ hệ thống.
            user.FullName = model.FullName;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                    return (false, "Vui lòng nhập mật khẩu hiện tại để xác nhận.");

                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
                    return (false, "Mật khẩu hiện tại không đúng.");

                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            // 2. Cập nhật bảng Customer (CHỈ KHI LÀ CUSTOMER)
            if (model.Role == "customer")
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.UserId);

                if (customer == null)
                {
                    customer = new Customer
                    {
                        UserId = user.UserId,
                        Name = model.FullName,
                        Phone = model.Phone,
                        Email = model.Email,
                        Address = model.Address,
                        CreatedAt = DateTime.Now
                    };
                    _context.Customers.Add(customer);
                }
                else
                {
                    customer.Name = model.FullName;
                    customer.Phone = model.Phone;
                    customer.Email = model.Email;
                    customer.Address = model.Address;
                }
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return (true, "Cập nhật hồ sơ thành công!");
        }

        // ... (Giữ nguyên các phần còn lại của file: GetAllUsersAsync, CreateUserAsync, etc.)
        // Các method phía dưới không cần đổi nếu không có yêu cầu khác.
        // Bạn copy lại phần dưới của file cũ vào đây.
        public async Task<List<UserAdminViewModel>> GetAllUsersAsync()
        {
            var users = await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
            var customerProfiles = await _context.Customers
                .Where(c => c.UserId.HasValue)
                .ToDictionaryAsync(c => c.UserId!.Value, c => c);

            var result = new List<UserAdminViewModel>();
            foreach (var u in users)
            {
                var profile = customerProfiles.ContainsKey(u.UserId) ? customerProfiles[u.UserId] : null;

                result.Add(new UserAdminViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName ?? "",
                    Role = u.Role ?? "staff",
                    Phone = profile?.Phone,
                    Email = profile?.Email,
                    Address = profile?.Address
                });
            }
            return result;
        }

        public async Task<UserAdminViewModel?> GetUserForEditAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;
            var profile = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == id);
            return new UserAdminViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName ?? "",
                Role = user.Role ?? "staff",
                Phone = profile?.Phone,
                Email = profile?.Email,
                Address = profile?.Address
            };
        }

        public async Task<(bool success, string message)> CreateUserAsync(UserAdminViewModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                return (false, "Tên đăng nhập đã tồn tại.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newUser = new User
                {
                    Username = model.Username,
                    FullName = model.FullName,
                    Role = model.Role,
                    CreatedAt = DateTime.Now
                };
                string passToHash = string.IsNullOrEmpty(model.Password) ? "123456" : model.Password;
                newUser.Password = BCrypt.Net.BCrypt.HashPassword(passToHash);

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                if (model.Role == "customer")
                {
                    var newCustomer = new Customer
                    {
                        UserId = newUser.UserId,
                        Name = model.FullName,
                        Phone = model.Phone,
                        Email = model.Email,
                        Address = model.Address,
                        CreatedAt = DateTime.Now
                    };
                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return (true, "Tạo tài khoản thành công!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateUserAsync(UserAdminViewModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null) return (false, "Tài khoản không tồn tại.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                user.FullName = model.FullName;
                user.Role = model.Role;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }
                _context.Users.Update(user);

                var existingProfile = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.UserId);
                if (model.Role == "customer")
                {
                    if (existingProfile == null)
                    {
                        existingProfile = new Customer { UserId = user.UserId, CreatedAt = DateTime.Now };
                        _context.Customers.Add(existingProfile);
                    }
                    existingProfile.Name = model.FullName;
                    existingProfile.Phone = model.Phone;
                    existingProfile.Email = model.Email;
                    existingProfile.Address = model.Address;
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Cập nhật thành công!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                var profiles = await _context.Customers.Where(c => c.UserId == id).ToListAsync();
                _context.Customers.RemoveRange(profiles);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}