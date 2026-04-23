using proekt.Services;
using proekt.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register SQL Server DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Core services (Scoped — share DbContext lifetime per request) ──────────────
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MedicalDocumentService>();
builder.Services.AddScoped<DoctorApplicationService>();
builder.Services.AddScoped<ContactInquiryService>();
builder.Services.AddScoped<MedicalRecordService>();

// ── New feature services ───────────────────────────────────────────────────────
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<PrescriptionService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<DoctorProfileService>();
builder.Services.AddScoped<ActivityLogService>();
builder.Services.AddScoped<StatisticsService>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<TranslationService>();

var app = builder.Build();

// ── Auto-apply migrations on startup ─────────────────────────────────────────
// Wrapped in try/catch so a missing SQL Server shows a clear error instead of
// crashing the process with exit code 134.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        Console.WriteLine("✅ Database connection successful and migrations applied.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ CRITICAL: Could not connect to SQL Server.");
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine("Troubleshooting Tips:");
        Console.WriteLine("1. Ensure Docker is running.");
        Console.WriteLine("2. Run 'docker-compose up -d' in the project root.");
        Console.WriteLine("3. Check if your connection string in appsettings.json uses 127.0.0.1,1435.");
        Console.WriteLine("4. Wait 10-15 seconds for SQL Server to fully initialize inside Docker.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
