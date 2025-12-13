using System;
using System.Collections.Generic;

namespace StoreManagementBlazor.Models;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int? CartId { get; set; }

    public int? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Subtotal { get; set; }
}
