using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using proekt.Data;
using proekt.Services;

var builder = WebApplication.CreateBuilder(args);

// DB Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("://") && connectionString.Contains("postgres"))
{
    // Convert Render's postgres://user:password@host/database format to Npgsql format
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    var user = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Data Protection (Persist keys to DB for Render)
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("MedReports_Diploma_Project");

// MVC
builder.Services.AddControllersWithViews();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Services (Scoped)
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MedicalDocumentService>();
builder.Services.AddScoped<DoctorApplicationService>();
builder.Services.AddScoped<ContactInquiryService>();
builder.Services.AddScoped<MedicalRecordService>();

builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<PrescriptionService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<DoctorProfileService>();
builder.Services.AddScoped<ActivityLogService>();
builder.Services.AddScoped<StatisticsService>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<TranslationService>();

var app = builder.Build();

// Auto migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("⏳ Applying database migrations...");
        db.Database.Migrate();
        Console.WriteLine("✅ Database connected and migrations applied.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ DB connection or migration failed!");
        Console.WriteLine($"Error: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
        }
    }
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS is handled by Render's edge — do not enable HSTS inside the container
}
else
{
    // In development, redirect to HTTPS if running locally with HTTPS
    app.UseHttpsRedirection();
}

app.UseStaticFiles();   // ✅ REPLACEMENT for MapStaticAssets

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();