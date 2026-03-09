using Microsoft.Extensions.DependencyInjection;
using proekt.Services;

namespace proekt
{
    public static class ServiceRegistration
    {
        public static void AddProjectServices(this IServiceCollection services)
        {
            services.AddScoped<UserService>(); // Scoped because UserService now depends on ApplicationDbContext
            services.AddSingleton<MedicalDocumentService>();
            services.AddSingleton<DoctorApplicationService>();
            services.AddSingleton<TranslationService>();
            services.AddSingleton<ContactInquiryService>();
        }
    }
}