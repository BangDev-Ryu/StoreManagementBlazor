// Program.cs (giữ nguyên như trước, nhưng thêm comment để rõ)
using Blazored.Toast;
using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Components;
using StoreManagementBlazor.Models;  // <-- Thêm nếu cần reference DbContext ở đây (thường không cần)
using StoreManagementBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// ================= DB CONTEXT =================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

// ================= SERVICES =================
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();