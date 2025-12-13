using System;

namespace StoreManagementBlazor.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;     // ✅ Navigation về Order

    public int? ProductId { get; set; }
    public Product? Product { get; set; }         // ✅ Navigation về Product

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Subtotal { get; set; }
}
