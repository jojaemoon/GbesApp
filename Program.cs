using GBES.Data;
using GBES.Managers;
using GBES.Models;
using GBES.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;


// 1MB
const long OneMB = 1L * 1024 * 1024; // 1,048,576

// 0.5MB (1/2로 줄이기)
const long HalfMB = OneMB / 2;       // 524,288
// 또는
//const long HalfMB2 = 512L * 1024L;   // 524,288
// 또는
//const long HalfMB3 = 1L * 1024 * 1024 / 2;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = HalfMB);
builder.Services.AddServerSideBlazor().AddHubOptions(o => o.MaximumReceiveMessageSize = HalfMB);
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = HalfMB);

// [START] GWSportsAll 
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));

builder.Services.AddDbContextFactory<GBESportsAppDbContext>(options => options.UseSqlServer(connectionString));

//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();

//builder.Services.AddControllersWithViews();

//builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

builder.Services.AddTransient<IGBESportsRepository, GBESportsRepositoty>();

//builder.Services.AddHttpClient();

//builder.Services.AddBlazoredSessionStorage();

// 전역변수 서비스 등록
builder.Services.AddScoped<AppState>();

// [END] GWSportsAll 

// blazor 쿠키 로그인을 위한 종속성 주입 항목
builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<HttpContextAccessor>();
//builder.Services.AddHttpClient();
//builder.Services.AddScoped<HttpContext>();
// 쿠키 인증 사용
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddAuthentication(
                CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    //options.LoginPath = "/auth/login";
                    //options.AccessDeniedPath = "/auth/accessdenied";
                    options.Cookie.IsEssential = true;
                    options.SlidingExpiration = true; // here 1
                    options.ExpireTimeSpan = TimeSpan.FromSeconds(10);// here 2
                });

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// 업로드 관련
builder.Services.AddScoped<IFileStorageManager, FileStrogeManager>();   // Local upload

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// 쿠키인증처리를 위한 종속성 주입
app.UseAuthentication();
app.UseAuthorization();

app.Run();
