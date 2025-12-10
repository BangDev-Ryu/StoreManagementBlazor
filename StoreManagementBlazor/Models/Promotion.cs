using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagementBlazor.Models;

public partial class Promotion : IValidatableObject
{
    public int PromoId { get; set; }

    // Mã khuyến mãi: BẮT BUỘC, duy nhất, độ dài 1-50
    [Required(ErrorMessage = "Mã khuyến mãi không được để trống.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Mã khuyến mãi phải từ 1 đến 50 ký tự.")]
    public string PromoCode { get; set; } = null!;

    // Mô tả: Không bắt buộc
    [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn loại giảm giá.")]
    [RegularExpression(@"^(percent|fixed)$", ErrorMessage = "Loại giảm giá chỉ có thể là 'phần trăm (%)' hoặc 'Số tiền'.")]
    public string DiscountType { get; set; } = null!;

    // Giá trị giảm: BẮT BUỘC, >= 0
    [Required(ErrorMessage = "Giá trị giảm không được để trống.")]
    public decimal? DiscountValue { get; set; }

    // Ngày bắt đầu: BẮT BUỘC
    [Required(ErrorMessage = "Ngày bắt đầu không được để trống.")]
    public DateTime StartDate { get; set; }

    // Ngày kết thúc: BẮT BUỘC
    [Required(ErrorMessage = "Ngày kết thúc không được để trống.")]
    public DateTime EndDate { get; set; }

    // Giá trị đơn hàng tối thiểu: Không bắt buộc, nhưng nếu có thì >= 0
    [Range(0, double.MaxValue, ErrorMessage = "Giá trị tối thiểu đơn hàng phải ≥ 0.")]
    public decimal? MinOrderAmount { get; set; } = 0;

    // Giới hạn sử dụng: Không bắt buộc, nếu có thì >= 0 (0 = không giới hạn)
    [Range(0, int.MaxValue, ErrorMessage = "Số lần sử dụng phải ≥ 0 (0 = không giới hạn).")]
    public int? UsageLimit { get; set; } = 0;

    // Số lần đã dùng: Tự động tăng, không cần nhập
    public int? UsedCount { get; set; } = 0;

    // Trạng thái: BẮT BUỘC, chỉ nhận "active" hoặc "inactive"
    [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
    [RegularExpression(@"^(active|inactive)$", ErrorMessage = "Trạng thái không hợp lệ.")]
    public string? Status { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // --- Validate theo loại giảm giá ---
        if (DiscountType == "percent")
        {
            if (!DiscountValue.HasValue)
            {
                yield return new ValidationResult(
                    "Vui lòng nhập giá trị giảm.",
                    [nameof(DiscountValue)]);
            }
            else if (DiscountValue < 0 || DiscountValue > 100)
            {
                yield return new ValidationResult(
                    "Giảm phần trăm phải từ 0 đến 100.",
                    [nameof(DiscountValue)]);
            }
        }

        if (DiscountType == "fixed")
        {
            if (!DiscountValue.HasValue)
            {
                yield return new ValidationResult(
                    "Vui lòng nhập giá trị giảm.",
                    [nameof(DiscountValue)]);
            }
            else if (DiscountValue < 10000)
            {
                yield return new ValidationResult(
                    "Giảm theo số tiền phải ≥ 10.000.",
                    [nameof(DiscountValue)]);
            }
            else if (DiscountValue > 10_000_000)
            {
                yield return new ValidationResult(
                    "Giá trị giảm không được vượt quá 10.000.000.",
                    [nameof(DiscountValue)]);
            }
        }

        // --- Giới hạn min_order_amount ---

        if (MinOrderAmount > 99_999_999)
        {
            yield return new ValidationResult(
                "Giá trị tối thiểu đơn hàng không được vượt quá 99.999.999.",
                [nameof(MinOrderAmount)]);
        }

        // --- usage limit ---
        if (UsageLimit.HasValue && UsageLimit.Value > 1_000_000)
        {
            yield return new ValidationResult(
              "Số lần sử dụng không được vượt quá 1.000.000 lần.",
              [nameof(UsageLimit)]);
        }

        // --- Validate ngày ---
        if (StartDate.Date >= EndDate.Date)
        {
            yield return new ValidationResult(
                "Ngày kết thúc phải lớn hơn ngày bắt đầu.",
                [nameof(EndDate)]);
        }
    }
}
