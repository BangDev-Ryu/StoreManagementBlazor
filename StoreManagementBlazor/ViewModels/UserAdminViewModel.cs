using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace StoreManagementBlazor.Models.ViewModels
{
    public class UserAdminViewModel : IValidatableObject
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Username { get; set; } = string.Empty;

        public string? Password { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string Role { get; set; } = "staff";

        // Bỏ các attribute validate đơn giản, chuyển vào hàm Validate bên dưới
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        // Logic kiểm tra tùy chỉnh
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Chỉ kiểm tra kỹ khi Role là Customer
            if (Role == "customer")
            {
                // 1. Kiểm tra Số điện thoại
                if (string.IsNullOrWhiteSpace(Phone))
                {
                    yield return new ValidationResult("Số điện thoại không được để trống.", new[] { nameof(Phone) });
                }
                else
                {
                    // Regex: Bắt đầu bằng 0, theo sau là 9 chữ số (tổng 10 số)
                    if (!Regex.IsMatch(Phone, @"^0\d{9}$"))
                    {
                        yield return new ValidationResult("SĐT phải gồm 10 chữ số và bắt đầu bằng số 0.", new[] { nameof(Phone) });
                    }
                }

                // 2. Kiểm tra Email
                if (string.IsNullOrWhiteSpace(Email))
                {
                    yield return new ValidationResult("Email không được để trống.", new[] { nameof(Email) });
                }
                else
                {
                    // Regex: Đuôi phải là @gmail.com
                    if (!Regex.IsMatch(Email, @"@gmail\.com$", RegexOptions.IgnoreCase))
                    {
                        yield return new ValidationResult("Email phải có định dạng @gmail.com.", new[] { nameof(Email) });
                    }
                }

                // 3. Kiểm tra Địa chỉ
                if (string.IsNullOrWhiteSpace(Address))
                {
                    yield return new ValidationResult("Địa chỉ không được để trống.", new[] { nameof(Address) });
                }
            }
        }
    }
}