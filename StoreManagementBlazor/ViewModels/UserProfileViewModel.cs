using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace StoreManagementBlazor.Models.ViewModels
{
    public class UserProfileViewModel : IValidatableObject
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = "staff"; // Thêm trường Role

        public DateTime CreatedAt { get; set; }

        // Các trường này chỉ hiện khi là Customer
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        // Đổi mật khẩu
        public string? CurrentPassword { get; set; }
        
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? ConfirmNewPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Chỉ bắt lỗi khi là Customer
            if (Role == "customer")
            {
                if (string.IsNullOrWhiteSpace(Phone))
                {
                    yield return new ValidationResult("Số điện thoại là bắt buộc.", new[] { nameof(Phone) });
                }
                else if (!Regex.IsMatch(Phone, @"^0\d{9}$"))
                {
                    yield return new ValidationResult("SĐT phải gồm 10 số và bắt đầu bằng 0.", new[] { nameof(Phone) });
                }

                if (string.IsNullOrWhiteSpace(Email))
                {
                    yield return new ValidationResult("Email là bắt buộc.", new[] { nameof(Email) });
                }
                else if (!Regex.IsMatch(Email, @"@gmail\.com$", RegexOptions.IgnoreCase))
                {
                    yield return new ValidationResult("Email phải có đuôi @gmail.com.", new[] { nameof(Email) });
                }

                if (string.IsNullOrWhiteSpace(Address))
                {
                    yield return new ValidationResult("Địa chỉ là bắt buộc.", new[] { nameof(Address) });
                }
            }
        }
    }
}