using Blazored.Toast;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Components;
using StoreManagementBlazor.Models; 
using StoreManagementBlazor.Services;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using StoreManagementBlazor.ViewModels; 
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ================= DB CONTEXT =================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

// ================= SERVICES =================
// Authentication: Cookie (simple)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // Security: cookie hardening
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;

        options.Cookie.Name = "StoreMgmtAuth";
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<PromotionService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OrdersService>(); // <-- Đã thêm
builder.Services.AddScoped<PaymentsService>(); // <-- Đã thêm

builder.Services.AddBlazoredToast();
builder.Services.AddScoped<InventoryService>();


// ================= BLAZOR =================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ================= PIPELINE =================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// === THÊM ĐOẠN NÀY ===
// Tạo một endpoint HTTP GET để xử lý đăng xuất
app.MapGet("/logout", async (Microsoft.AspNetCore.Http.HttpContext context) =>
{
    // Xóa cookie xác thực
    await context.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
    
    // Chuyển hướng về trang đăng nhập
    return Results.Redirect("/login");
});
// =====================

// Endpoint xử lý đăng nhập dạng Form POST
app.MapPost("/login-handler", async (HttpContext context, [FromForm] LoginViewModel model, ApplicationDbContext db) =>
{
    // 1. Kiểm tra user trong DB
    var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == model.Username);

    if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
    {
        // Đăng nhập thất bại: Quay lại trang login với thông báo lỗi
        return Results.Redirect("/login?error=InvalidCredentials");
    }

    // 2. Tạo Claims
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role ?? "User"),
        new Claim("FullName", user.FullName ?? "")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    // 3. Ghi Cookie xác thực (Quan trọng: Hoạt động ổn định tại đây)
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
        new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        });

    // 4. Chuyển hướng về trang chủ
    return Results.Redirect("/");
});
// =====================

app.Run();