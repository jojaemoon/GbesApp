using GBES.Data;
using GBES.Managers;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

const long OneMB = 1L * 1024 * 1024;
const long HalfMB = OneMB / 2;

var builder = WebApplication.CreateBuilder(args);

// 업로드/SignalR 제한
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = HalfMB;
});

builder.Services
    .AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = HalfMB;
    });

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = HalfMB;
});

// DB
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<GBESportsAppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repository / Services
builder.Services.AddTransient<IGBESportsRepository, GBESportsRepositoty>();
builder.Services.AddScoped<AppState>();
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.IsEssential = true;
        options.SlidingExpiration = true;

        // 기존 10초는 너무 짧음.
        // 실제 요구사항에 따라 조정.
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization();

builder.Services.AddRazorPages();
builder.Services.AddSingleton<WeatherForecastService>();

// 파일 업로드
builder.Services.AddScoped<IFileStorageManager, FileStrogeManager>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();