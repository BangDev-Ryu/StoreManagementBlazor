using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagementBlazor.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public int? PromoId { get; set; }

    [ForeignKey(nameof(PromoId))]          // ✅ QUAN TRỌNG NHẤT
    public Promotion? Promotion { get; set; }

    public Payment? Payment { get; set; }

    public ICollection<OrderItem> Items { get; set; }
        = new List<OrderItem>();

    public DateTime OrderDate { get; set; }

    public string? Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }
}
