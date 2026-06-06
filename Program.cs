using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Literasi.Data;

const string AppAuthScheme = "LiterasiAuth";
const string AdminAuthScheme = "AdminAuth";
const string TeacherAuthScheme = "TeacherAuth";
const string StudentAuthScheme = "StudentAuth";

var builder = WebApplication.CreateBuilder(args);

// Konfigurasi Max Upload Size (100MB) — untuk modul unggah bahan ajar OER
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
});

// Koneksi MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Razor Pages
builder.Services.AddRazorPages();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Authentication: separate cookies allow Admin, Guru, and Siswa sessions
// to stay logged in at the same time in one browser.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = AppAuthScheme;
        options.DefaultChallengeScheme = AppAuthScheme;
    })
    .AddPolicyScheme(AppAuthScheme, AppAuthScheme, options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var path = context.Request.Path;

            if (path.StartsWithSegments("/Admin"))
                return AdminAuthScheme;

            if (path.StartsWithSegments("/Teacher"))
                return TeacherAuthScheme;

            if (path.StartsWithSegments("/Student"))
                return StudentAuthScheme;

            return AdminAuthScheme;
        };
    })
    .AddCookie(AdminAuthScheme, options =>
    {
        options.Cookie.Name = "LITERASI.Admin";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddCookie(TeacherAuthScheme, options =>
    {
        options.Cookie.Name = "LITERASI.Teacher";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddCookie(StudentAuthScheme, options =>
    {
        options.Cookie.Name = "LITERASI.Student";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
