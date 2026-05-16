using proekt.Services;
using proekt.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        db.Database.Migrate();
        Console.WriteLine("✅ Database connected and migrations applied.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ DB connection failed.");
        Console.WriteLine(ex.Message);
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