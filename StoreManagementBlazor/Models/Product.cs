using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagementBlazor.Models
{
    public partial class Product
    {
        [Key]
        public int ProductId { get; set; }

        // ===== FK =====
        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }

        [Display(Name = "Nhà cung cấp")]
        public int? SupplierId { get; set; }

        // ===== THÔNG TIN SẢN PHẨM =====
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm tối đa 200 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Mã vạch tối đa 50 ký tự")]
        [Display(Name = "Mã vạch")]
        public string? Barcode { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Đơn vị tính không được để trống")]
        [StringLength(50, ErrorMessage = "Đơn vị tối đa 50 ký tự")]
        [Display(Name = "Đơn vị")]
        public string? Unit { get; set; }

        // ===== HỆ THỐNG =====
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
