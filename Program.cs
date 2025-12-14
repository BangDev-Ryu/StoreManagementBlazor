using Blazored.Toast;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Components;
using StoreManagementBlazor.Models;
using StoreManagementBlazor.Services;
using System.Security.Claims;

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

// Register AuthService as a typed HTTP client so HttpClient is injected automatically.
// Set BaseAddress from configuration if you have an API URL, otherwise fallback to localhost.
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApiBase") ?? "https://localhost:5001/");
});

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<PromotionService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddBlazoredToast();

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
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();