using System.ComponentModel.DataAnnotations;

public class Category
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Tên danh mục không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên danh mục không quá 100 ký tự.")]
    public string CategoryName { get; set; } = string.Empty;

    
}
