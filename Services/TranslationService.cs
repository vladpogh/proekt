using Microsoft.AspNetCore.Http;

namespace proekt.Services;

public class TranslationService
{
    private readonly IHttpContextAccessor _http;
    private const string SessionKey = "Lang";

    private readonly Dictionary<string, string> _nativeNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = "English",
        ["bg"] = "Български",
    };

    private readonly Dictionary<string, string> _flags = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = "🇬🇧",
        ["bg"] = "🇧🇬",
    };

    private readonly Dictionary<string, Dictionary<string, string>> _data;

    public TranslationService(IHttpContextAccessor http)
    {
        _http = http;
        _data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new Dictionary<string, string>
            {
                ["Home"] = "Home",
                ["Login"] = "Login",
                ["Register"] = "Register",
                ["EHRSystem"] = "EHR System",
                ["EHRDescription"] = "Electronic medical records management system",
                ["Email"] = "Email",
                ["Password"] = "Password",
                ["DemoAccount"] = "Demo account:",
                ["DemoEmail"] = "Email: admin@example.com",
                ["DemoPassword"] = "Password: 1234",
                ["About Us"] = "About Us",
                ["Medical Record"] = "Medical Record",
                ["Contact"] = "Contact",
                ["AdminPanel"] = "Admin Panel",
                ["MyProfile"] = "My Profile",
                ["Logout"] = "Logout",
                ["PassRequirements"] = "Password Requirements",
                ["PassLength"] = "At least 8 characters",
                ["PassSpecial"] = "At least 1 special character",
                ["PassNumber"] = "At least 1 number",
                ["PassNot1234"] = "Cannot be '1234'",
                ["DoctorApplication"] = "Doctor Application",
                ["TellUsMore"] = "Tell us about your experience",
                ["EmploymentContract"] = "Employment Contract",
                ["IDCard"] = "ID Card (Front & Back)",
                ["MedicalLicense"] = "Medical License",
                ["SubmitApplication"] = "Submit Application",
                ["AppDescription"] = "Join our network of healthcare professionals. Please provide the required documents for verification.",
                ["FileUpload"] = "Click to upload or drag and drop",
                ["FileRequired"] = "PDF, JPG or PNG (max. 10MB)",
                ["Index_HeroBadge"] = "Innovative medical solutions",
                ["Index_HeroTitle"] = "Medical<br/>records<br/>systems",
                ["Index_HeroDesc"] = "Professional solutions for healthcare facilities and clinics. Quality, security, and innovation in a single system.",
                ["Index_ShowRecord"] = "Show record →",
                ["Index_ContactUs"] = "Contact us",
                ["Index_Adv_Title"] = "Our product advantages",
                ["Index_Adv_1"] = "Wide range of solutions for various healthcare facilities",
                ["Index_Adv_2"] = "Compliance with GDPR and health standards",
                ["Index_Adv_3"] = "Competitive prices and rapid deployment",
                ["Index_Adv_4"] = "Customized solutions according to clinic needs",
                ["Index_Adv_5"] = "Long-term support and updates",
                ["Index_Stats_Records"] = "Medical records",
                ["Index_Stats_Doctors"] = "Specialists",
                ["Index_Stats_Users"] = "Users",
                ["About_Mission_Title"] = "Our Mission",
                ["About_Mission_Desc"] = "At MedReports, we believe that access to healthcare should be easy, secure, and digitized. Our goal is to transform how patients and doctors interact through innovative technological solutions.",
                ["About_Story_Title"] = "Our Story",
                ["About_Story_P1"] = "Founded in 2024, MedReports started as a small initiative with one vision: to put an end to paper documentation in hospitals. Today, we serve thousands of patients and doctors nationwide, offering a platform that is both powerful and user-friendly.",
                ["About_Story_P2"] = "Every feature in our system is designed with the end-user in mind, passing through rigorous security and quality tests.",
                ["About_Values_Title"] = "Our Values",
                ["About_Values_Sec_Title"] = "Security",
                ["About_Values_Sec_Desc"] = "Your data is protected with state-of-the-art encryption and security methods.",
                ["About_Values_Inn_Title"] = "Innovation",
                ["About_Values_Inn_Desc"] = "We continuously improve our products according to the latest technology trends.",
                ["About_Values_Qual_Title"] = "Quality",
                ["About_Values_Qual_Desc"] = "We provide the best user experience for medical administration.",
                ["About_CTA_Title"] = "Ready to get started?",
                ["About_CTA_Desc"] = "Join thousands of satisfied users today.",
                ["Contact_Header_Title"] = "Contact us",
                ["Contact_Header_Desc"] = "Send us your inquiry and we will respond as soon as possible.",
                ["Contact_Form_Name"] = "Name *",
                ["Contact_Form_NamePH"] = "Your name",
                ["Contact_Form_Email"] = "Email *",
                ["Contact_Form_EmailPH"] = "Your email",
                ["Contact_Form_Phone"] = "Phone",
                ["Contact_Form_PhonePH"] = "Your phone",
                ["Contact_Form_Subj"] = "Subject *",
                ["Contact_Form_SubjPH"] = "Inquiry subject",
                ["Contact_Form_Msg"] = "Message *",
                ["Contact_Form_MsgPH"] = "Your message",
                ["Contact_Form_Btn"] = "Send message",
                ["Contact_Info_Addr_Title"] = "Address",
                ["Contact_Info_Addr_Desc"] = "Sofia, Vitosha Blvd 123",
                ["Contact_Info_Phone_Title"] = "Phone",
                ["Contact_Info_Phone_Desc"] = "+359 2 123 4567",
                ["Contact_Info_Email_Title"] = "Email",
                ["Contact_Info_Email_Desc"] = "info@medreports.bg",
                ["Contact_Info_Hours_Title"] = "Working Hours",
                ["Contact_Info_Hours_Desc"] = "Mon-Fri: 9:00-18:00",
                ["Contact_Map"] = "Google Maps integration",
                ["Footer_CTA_Q"] = "Are you a doctor? Get your rights here!",
                ["Footer_CTA_Btn"] = "Apply Now",
                ["Footer_Brand_Desc"] = "Professional solutions for medical records and health documentation.",
                ["Footer_Nav_Title"] = "NAVIGATION",
                ["Footer_Support_Title"] = "SUPPORT",
                ["Footer_Contact_Title"] = "CONTACTS",
            },
            ["bg"] = new Dictionary<string, string>
            {
                ["Home"] = "Начало",
                ["Login"] = "Вход",
                ["Register"] = "Регистрация",
                ["EHRSystem"] = "EHR Система",
                ["EHRDescription"] = "Електронна медицинска система за управление на здравни досиета",
                ["Email"] = "Имейл",
                ["Password"] = "Парола",
                ["DemoAccount"] = "Демо акаунт:",
                ["DemoEmail"] = "Email: admin@example.com",
                ["DemoPassword"] = "Парола: 1234",
                ["About Us"] = "За нас",
                ["Medical Record"] = "Мед. Досие",
                ["Contact"] = "Контакти",
                ["AdminPanel"] = "Админ Панел",
                ["MyProfile"] = "Моят Профил",
                ["Logout"] = "Изход",
                ["PassRequirements"] = "Изисквания за парола",
                ["PassLength"] = "Поне 8 символа",
                ["PassSpecial"] = "Поне 1 специален символ",
                ["PassNumber"] = "Поне 1 цифра",
                ["PassNot1234"] = "Не може да бъде '1234'",
                ["DoctorApplication"] = "Кандидатура за лекар",
                ["TellUsMore"] = "Разкажете ни за вашия опит",
                ["EmploymentContract"] = "Трудов договор",
                ["IDCard"] = "Лична карта (двустранно)",
                ["MedicalLicense"] = "Медицински лиценз",
                ["SubmitApplication"] = "Изпрати кандидатура",
                ["AppDescription"] = "Присъединете се към нашата мрежа. Моля, предоставете необходимите документи за проверка.",
                ["FileUpload"] = "Кликнете за качване или плъзнете тук",
                ["FileRequired"] = "PDF, JPG или PNG (макс. 10MB)",
                ["Index_HeroBadge"] = "Иновативни медицински решения",
                ["Index_HeroTitle"] = "Системи за<br/>медицински<br/>досиета",
                ["Index_HeroDesc"] = "Професионални решения за здравни заведения и клиники. Качество, сигурност и иновации в една система.",
                ["Index_ShowRecord"] = "Покажи досие →",
                ["Index_ContactUs"] = "Свържете се с нас",
                ["Index_Adv_Title"] = "Преимства на нашите продукти",
                ["Index_Adv_1"] = "Широка гама от решения за различни здравни заведения",
                ["Index_Adv_2"] = "Съответствие с GDPR и здравните стандарти",
                ["Index_Adv_3"] = "Конкурентни цени и брзо внедряване",
                ["Index_Adv_4"] = "Индивидуални решения според нужите на клиниката",
                ["Index_Adv_5"] = "Дългогодишна поддръжка и обновления",
                ["Index_Stats_Records"] = "Медицински записи",
                ["Index_Stats_Doctors"] = "Специалисти",
                ["Index_Stats_Users"] = "Потребители",
                ["About_Mission_Title"] = "Нашата Мисия",
                ["About_Mission_Desc"] = "Ние в MedReports вярваме, че достъпът до здравеопазване трябва да бъде лесен, сигурен и дигитализиран. Нашата цел е да преобразим начина, по който пациентите и лекарите взаимодействат, чрез иновативни технологични решения.",
                ["About_Story_Title"] = "Нашата История",
                ["About_Story_P1"] = "Основана през 2024 година, MedReports започна като малка инициатива с една визия: да сложи край на хартиената документация в болниците. Днес ние обслужваме хиляди пациенти и лекари в цялата страна, предлагайки платформа, която е едновременно мощна и лесна за употреба.",
                ["About_Story_P2"] = "Всяка функция в нашата система е разработена с мисъл за крайния потребител, преминавайки през строги тестове за сигурност и качество.",
                ["About_Values_Title"] = "Нашите Ценности",
                ["About_Values_Sec_Title"] = "Сигурност",
                ["About_Values_Sec_Desc"] = "Вашите данни са защитени с най-модерните методи за криптиране и сигурност.",
                ["About_Values_Inn_Title"] = "Иновация",
                ["About_Values_Inn_Desc"] = "Ние непрекъснато подобряваме нашите продукти според последните тенденции в технологиите.",
                ["About_Values_Qual_Title"] = "Качество",
                ["About_Values_Qual_Desc"] = "Предоставяме най-доброто потребителско изживяване за медицинска администрация.",
                ["About_CTA_Title"] = "Готови ли сте да започнете?",
                ["About_CTA_Desc"] = "Присъединете се към хилядите доволни потребители днес.",
                ["Contact_Header_Title"] = "Свържете се с нас",
                ["Contact_Header_Desc"] = "Изпратете ни вашето запитване и щё отговорим възможно най-скоро",
                ["Contact_Form_Name"] = "Име *",
                ["Contact_Form_NamePH"] = "Вашето име",
                ["Contact_Form_Email"] = "Имейл *",
                ["Contact_Form_EmailPH"] = "Ваш имейл",
                ["Contact_Form_Phone"] = "Телефон",
                ["Contact_Form_PhonePH"] = "Ваш телефон",
                ["Contact_Form_Subj"] = "Тема *",
                ["Contact_Form_SubjPH"] = "Тема на запитването",
                ["Contact_Form_Msg"] = "Съобщение *",
                ["Contact_Form_MsgPH"] = "Вашето съобщение",
                ["Contact_Form_Btn"] = "Изпрати съобщение",
                ["Contact_Info_Addr_Title"] = "Адрес",
                ["Contact_Info_Addr_Desc"] = "гр. София, бул. Витоша 123",
                ["Contact_Info_Phone_Title"] = "Телефон",
                ["Contact_Info_Phone_Desc"] = "+359 2 123 4567",
                ["Contact_Info_Email_Title"] = "Имейл",
                ["Contact_Info_Email_Desc"] = "info@medreports.bg",
                ["Contact_Info_Hours_Title"] = "Работно време",
                ["Contact_Info_Hours_Desc"] = "Пон-Пет: 9:00-18:00",
                ["Contact_Map"] = "Google Maps интеграция",
                ["Footer_CTA_Q"] = "Вие сте лекар? Вземете своите права тук!",
                ["Footer_CTA_Btn"] = "Кандидатствай сега",
                ["Footer_Brand_Desc"] = "Професионални решения за медицински досиета и здравна документация.",
                ["Footer_Nav_Title"] = "НАВИГАЦИЯ",
                ["Footer_Support_Title"] = "ПОДДРЪЖКА",
                ["Footer_Contact_Title"] = "КОНТАКТИ",
            }
        };
    }

    private string CurrentLang()
    {
        var ctx = _http.HttpContext;
        if (ctx == null) return "en";
        var lang = ctx.Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(lang)) return "en";
        return _data.ContainsKey(lang) ? lang : "en";
    }

    public string CurrentLanguageCode() => CurrentLang();

    public string CurrentNativeName()
    {
        var code = CurrentLang();
        if (_nativeNames.TryGetValue(code, out var name)) return name;
        return code.ToUpperInvariant();
    }

    public string CurrentNativeFlag()
    {
        var code = CurrentLang();
        if (_flags.TryGetValue(code, out var f)) return f;
        return "";
    }

    public string FlagFor(string code)
    {
        if (string.IsNullOrEmpty(code)) return "";
        if (_flags.TryGetValue(code, out var f)) return f;
        return "";
    }

    public string NativeNameFor(string code)
    {
        if (string.IsNullOrEmpty(code)) return code;
        if (_nativeNames.TryGetValue(code, out var name)) return name;
        return code.ToUpperInvariant();
    }

    public string T(string key)
    {
        var lang = CurrentLang();
        if (_data.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var val))
            return val;
        if (_data.TryGetValue("en", out var edict) && edict.TryGetValue(key, out var ev)) return ev;
        return key;
    }

    public void SetLanguage(string lang)
    {
        var ctx = _http.HttpContext;
        if (ctx == null) return;
        if (!_data.ContainsKey(lang)) lang = "en";
        ctx.Session.SetString(SessionKey, lang);
    }
}
