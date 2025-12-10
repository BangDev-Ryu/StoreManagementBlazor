using System;
using System.Collections.Generic;

namespace StoreManagementBlazor.Models;

public partial class Promotion
{
    public int PromoId { get; set; }

    public string PromoCode { get; set; } = null!;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal? MinOrderAmount { get; set; } = 0;

    public int? UsageLimit { get; set; } = 0;

    public int? UsedCount { get; set; } = 0;

    public string? Status { get; set; }
}
