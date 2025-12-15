using System;
using System.Collections.Generic;

namespace StoreManagementBlazor.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? CustomerId { get; set; }

    public string? Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
