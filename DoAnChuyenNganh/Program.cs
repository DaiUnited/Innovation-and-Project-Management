using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Thiết lập DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Thiết lập Authentication và Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Mã hóa tất cả mật khẩu chưa được mã hóa
    var users = context.NguoiDungs.ToList();
    foreach (var user in users)
    {
        if (!user.MatKhau.StartsWith("$2")) // Chỉ mã hóa nếu mật khẩu chưa mã hóa
        {
            user.MatKhau = BCrypt.Net.BCrypt.HashPassword(user.MatKhau);
        }
    }
    context.SaveChanges();
}


// Middleware pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Cấu hình cho khu vực Admin
app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

// Cấu hình cho khu vực BGK (Ban Giám Khảo)
app.MapAreaControllerRoute(
    name: "BGK",
    areaName: "BGK",
    pattern: "BGK/{controller=Home}/{action=Index}/{id?}");

// Cấu hình cho khu vực GVHD (Giảng Viên Hướng Dẫn)
app.MapAreaControllerRoute(
    name: "GVHD",
    areaName: "GVHD",
    pattern: "GVHD/{controller=Home}/{action=Index}/{id?}");

// Cấu hình cho khu vực SV (Sinh Viên)
app.MapAreaControllerRoute(
    name: "SV",
    areaName: "SV",
    pattern: "SV/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
