using Microsoft.Extensions.DependencyInjection;
using proekt.Services;

namespace proekt
{
    public static class ServiceRegistration
    {
        public static void AddProjectServices(this IServiceCollection services)
        {
            services.AddSingleton<UserService>();
            services.AddSingleton<MedicalDocumentService>();
            services.AddSingleton<DoctorApplicationService>();
            services.AddSingleton<TranslationService>();
            services.AddSingleton<ContactInquiryService>();
        }
    }
}