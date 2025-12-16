using System.ComponentModel.DataAnnotations;

namespace StoreManagementBlazor.Models.ViewModels
{
    // Cần thiết cho trang Create
    public class ProductSelectionDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string DisplayText => $"{ProductName} - {Price:N0}₫";
    }

    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    
    public class OrderCreationDTO
{
    public int? CustomerId { get; set; }

    public string? PromoCode { get; set; }

    [Required]
    public int UserId { get; set; }

    [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất 1 sản phẩm")]
    public List<CartItemDTO> CartItems { get; set; } = new();
}
    // Cần thiết cho trang Index
    public class OrderFilterDTO
    {
        public string SearchCustomer { get; set; } = string.Empty;
        public string SearchDate { get; set; } = string.Empty; 
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SortBy { get; set; } = "date"; 
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    
    public class OrderListDTO
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "pending";
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
    
    // Cần thiết cho trang Details và Delete
    public class OrderItemDTO
    {
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }
    
    public class OrderDetailsDTO
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }

    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }

    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "pending";
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? PromoCode { get; set; }

    public string? PaymentMethod { get; set; }
    public bool IsPaid { get; set; }
    public List<OrderItemDTO> Items { get; set; } = new();
}
}