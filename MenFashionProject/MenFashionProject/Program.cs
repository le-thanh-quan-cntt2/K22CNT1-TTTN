using MenFashionProject.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. CẤU HÌNH DỊCH VỤ ĐĂNG NHẬP (Authentication Service)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Chưa đăng nhập thì chuyển về đây
        options.AccessDeniedPath = "/Account/AccessDenied"; // Không được phép vào thì chuyển về đây
    });

// 2. Cấu hình dịch vụ Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Giỏ hàng tồn tại trong 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Kết nối CSDL (Lưu ý: Tên Context phải chuẩn là MenFashionContext như đã tạo ở bước trước)
builder.Services.AddDbContext<MenFashionContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MenFashionContext")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 4. Kích hoạt Session (Đặt TRƯỚC Routing)
app.UseSession();

app.UseRouting();

// 5. Kích hoạt Authentication & Authorization (Thứ tự cực kỳ quan trọng)
app.UseAuthentication(); // <--- BẮT BUỘC PHẢI CÓ DÒNG NÀY (Đặt trước Authorization)
app.UseAuthorization();

// 6. Cấu hình Route
// Route cho Admin (Ưu tiên chạy trước)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Route cho Khách hàng (Mặc định)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();