using System;
using System.Collections.Generic;

namespace StoreManagementBlazor.Models;

using System.ComponentModel.DataAnnotations.Schema;

public class CartItem
{
    public int CartItemId { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }

    public Product Product { get; set; }

    [NotMapped]
    public bool IsSelected { get; set; } // chỉ dùng cho UI
}
