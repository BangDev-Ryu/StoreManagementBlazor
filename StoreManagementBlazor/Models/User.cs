using System;
using System.Collections.Generic;

namespace StoreManagementBlazor.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Role { get; set; }

    public DateTime CreatedAt { get; set; }
}
