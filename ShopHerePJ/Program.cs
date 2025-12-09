using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Data.Entities; // namespace chứa ShopHereContext

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký DbContext dùng connection string trong appsettings.json
builder.Services.AddDbContext<ShopHereContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ShopHereConnection")
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Nếu có Area: Admin thì nên map route area trước
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
