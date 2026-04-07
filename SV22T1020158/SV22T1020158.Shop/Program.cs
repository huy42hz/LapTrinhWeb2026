using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SV22T1020158.Admin;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
    .AddMvcOptions(option =>
    {
        option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

// Configure Authentication for Shop (Customer)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "LiteCommerce.Shop";
        option.LoginPath = "/ShopAccount/Login";       
        option.LogoutPath = "/ShopAccount/Logout";
        option.AccessDeniedPath = "/ShopAccount/AccessDenied";
        option.ExpireTimeSpan = TimeSpan.FromDays(7);
        option.SlidingExpiration = true;
        option.Cookie.HttpOnly = true;
        option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Configure Authorization - YÊU CẦU ĐĂNG NHẬP CHO TOÀN BỘ ỨNG DỤNG
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()     
        .Build();
});

// Configure Session (for Cart)
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromHours(2);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();  // Quan trọng: phải ở trước UseAuthorization
app.UseAuthorization();   // Quan trọng: phải ở sau UseAuthentication
app.UseSession();

// Configure Routing for Shop
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ShopHome}/{action=Index}/{id?}");

// Configure default format (Vietnamese)
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Configure Application Context
ApplicationContext.Configure
(
    httpContextAccessor: app.Services.GetRequiredService<IHttpContextAccessor>(),
    webHostEnvironment: app.Services.GetRequiredService<IWebHostEnvironment>(),
    configuration: app.Configuration
);

// Get Connection String from appsettings.json
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB")
    ?? throw new InvalidOperationException("ConnectionString 'LiteCommerceDB' not found.");

// Initialize Business Layer Configuration
SV22T1020158.BusinessLayers.Configuration.Initialize(connectionString);

app.Run();