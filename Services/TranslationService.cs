using Microsoft.AspNetCore.Http;

namespace proekt.Services;

public class TranslationService
{
    private readonly IHttpContextAccessor _http;
    private readonly Dictionary<string, Dictionary<string, string>> _data;
    private const string SessionKey = "Lang";
    private readonly Dictionary<string,string> _nativeNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = "English",
        ["bg"] = "Български",
        ["ru"] = "Русский",
        ["es"] = "Español",
    };
    private readonly Dictionary<string,string> _flags = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = "🇬🇧",
        ["bg"] = "🇧🇬",
        ["ru"] = "🇷🇺",
        ["es"] = "🇪🇸",
    };

    public TranslationService(IHttpContextAccessor http)
    {
        _http = http;
        _data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new Dictionary<string,string>
            {
                ["Password"] = "Password",
                ["EHRSystem"] = "EHR System",
                ["EHRDescription"] = "Electronic medical records management system",
                ["NoAccountRegister"] = "Don't have an account?",
                ["HaveAccountLogin"] = "Already have an account?",
                ["DemoAccount"] = "Demo account:",
                ["DemoEmail"] = "Email: admin@example.com",
                ["DemoPassword"] = "Password: 1234",
                ["Home"] = "Home",
                ["Products"] = "Products",
                ["Solutions"] = "Solutions",
                ["Support"] = "Support",
                ["AboutUs"] = "About",
                ["Contact"] = "Contact",
                ["Login"] = "Login",
                ["Register"] = "Register",
                ["AdminPanel"] = "Admin Panel",
                ["DoctorApplication"] = "Doctor Application",
                ["MyProfile"] = "My Profile",
                ["ApplyNow"] = "Apply now",
                ["ApproveMedicalDocuments"] = "Approve Medical Documents",
                ["CloseUserProfiles"] = "Close User Profiles",
                ["MakeRemoveAdmin"] = "Make/Remove Admin",
                ["DoctorApplications"] = "Doctor Applications",
                ["ShowMore"] = "Show More",
                ["Approve"] = "Approve",
                ["Reject"] = "Reject",
                ["ViewFile"] = "View file",
                ["NotProvided"] = "Not provided",
                ["Logout"] = "Logout",
                ["Submitted"] = "Submitted",
                ["AdminComment"] = "Admin comment",
                ["PersonalInfo"] = "Personal Info",
                ["ChangePassword"] = "Change Password",
                ["ContactUs"] = "Contact Us",
                ["ApplicationStatus"] = "Doctor Application Status",
                ["SubmitApplication"] = "Submit Application",
                ["TellUsMore"] = "Tell us more about yourself",
                ["Email"] = "Email",
                ["Role"] = "Role",
                ["NewPassword"] = "New password",
                ["EmploymentContract"] = "Employment Contract",
                ["IDCard"] = "ID Card",
                ["MedicalLicense"] = "Medical License",
                ["NoPending"] = "No pending items.",
                ["Terminate"] = "Terminate",
                ["MakeAdmin"] = "Make Admin",
                ["RemoveAdmin"] = "Remove Admin",
            },
            ["bg"] = new Dictionary<string,string>
            {
                ["Password"] = "Парола",
                ["EHRSystem"] = "EHR Система",
                ["EHRDescription"] = "Електронна медицинска система за управление на здравни досиета",
                ["NoAccountRegister"] = "Не имате акаунт?",
                ["HaveAccountLogin"] = "Вече имате акаунт?",
                ["DemoAccount"] = "Демо акаунт:",
                ["DemoEmail"] = "Email: admin@example.com",
                ["DemoPassword"] = "Парола: 1234",
                ["Home"] = "Начало",
                ["Products"] = "Продукти",
                ["Solutions"] = "Решения",
                ["Support"] = "Поддръжка",
                ["AboutUs"] = "За нас",
                ["Contact"] = "Контакти",
                ["TellUsMore"] = "Кажете ни повече за себе си",
                ["Register"] = "Регистрация",
                ["AdminPanel"] = "Админ панел",
                ["DoctorApplication"] = "Заявка за доктор",
                ["MyProfile"] = "Моят профил",
                ["ApplyNow"] = "Кандидатствай сега",
                ["ApproveMedicalDocuments"] = "Одобряване на медицински документи",
                ["CloseUserProfiles"] = "Затваряне на профили",
                ["MakeRemoveAdmin"] = "Назначаване/Премахване на админ",
                ["DoctorApplications"] = "Заявки за доктори",
                ["ShowMore"] = "Виж повече",
                ["Approve"] = "Одобри",
                ["Reject"] = "Откажи",
                ["ViewFile"] = "Виж файла",
                ["NotProvided"] = "Не е предоставено",
                ["Logout"] = "Изход",
                ["Submitted"] = "Подадено",
                ["AdminComment"] = "Коментар на админа",
                ["PersonalInfo"] = "Лична информация",
                ["ChangePassword"] = "Смяна на парола",
                ["ContactUs"] = "Свържете се с нас",
                ["ApplicationStatus"] = "Статус на заявката за доктор",
                ["SubmitApplication"] = "Изпрати заявка",
                ["FullName"] = "Пълно име",
                ["Email"] = "Имейл",
                ["Role"] = "Роля",
                ["NewPassword"] = "Нова парола",
                ["TellUsMore"] = "Расскажите о себе",
                ["IDCard"] = "Лична карта",
                ["MedicalLicense"] = "Медицинска лиценция",
                ["NoPending"] = "Няма чакащи елементи.",
                ["Terminate"] = "Премахни",
                ["MakeAdmin"] = "Направи админ",
                ["RemoveAdmin"] = "Премахни админ",
            },
            ["ru"] = new Dictionary<string,string>
            {
                ["Password"] = "Пароль",
                ["EHRSystem"] = "EHR Система",
                ["EHRDescription"] = "Электронная медицинская система для управления медицинскими записями",
                ["NoAccountRegister"] = "Нет аккаунта?",
                ["HaveAccountLogin"] = "Уже есть аккаунт?",
                ["DemoAccount"] = "Демо аккаунт:",
                ["DemoEmail"] = "Email: admin@example.com",
                ["DemoPassword"] = "Пароль: 1234",
                ["Home"] = "Главная",
                ["Products"] = "Продукты",
                ["Solutions"] = "Решения",
                ["Support"] = "Поддержка",
                ["AboutUs"] = "О нас",
                ["Contact"] = "Контакты",
                ["Login"] = "Войти",
                ["Register"] = "Регистрация",
                ["AdminPanel"] = "Панель админа",
                ["DoctorApplication"] = "Заявка доктора",
                ["MyProfile"] = "Мой профиль",
                ["ApplyNow"] = "Подать заявку",
                ["ApproveMedicalDocuments"] = "Одобрить медицинские документы",
                ["CloseUserProfiles"] = "Закрыть профили",
                ["MakeRemoveAdmin"] = "Назначить/Удалить админа",
                ["DoctorApplications"] = "Заявки докторов",
                ["ShowMore"] = "Подробнее",
                ["Approve"] = "Одобрить",
                ["Reject"] = "Отклонить",
                ["ViewFile"] = "Просмотреть файл",
                ["NotProvided"] = "Не предоставлено",
                ["Logout"] = "Выйти",
                ["Submitted"] = "Отправлено",
                ["AdminComment"] = "Комментарий администратора",
                ["PersonalInfo"] = "Личная информация",
                ["ChangePassword"] = "Сменить пароль",
                ["ContactUs"] = "Связаться с нами",
                ["ApplicationStatus"] = "Статус заявки доктора",
                ["SubmitApplication"] = "Отправить заявку",
                ["FullName"] = "Полное имя",
                ["Email"] = "Эл. почта",
                ["Role"] = "Роль",
                ["NewPassword"] = "Новый пароль",
                ["EmploymentContract"] = "Трудовой договор",
                ["IDCard"] = "Удостоверение личности",
                ["MedicalLicense"] = "Медицинская лицензия",
                ["NoPending"] = "Нет ожидающих элементов.",
                ["Terminate"] = "Завершить",
                ["MakeAdmin"] = "Сделать админом",
                ["RemoveAdmin"] = "Убрать админа",
                ["TellUsMore"] = "Расскажите о себе",
            },
            ["es"] = new Dictionary<string,string>
            {
                ["Password"] = "Contraseña",
                ["EHRSystem"] = "Sistema EHR",
                ["EHRDescription"] = "Sistema electrónico de gestión de registros médicos",
                ["NoAccountRegister"] = "¿No tienes una cuenta?",
                ["HaveAccountLogin"] = "¿Ya tienes una cuenta?",
                ["DemoAccount"] = "Cuenta demo:",
                ["DemoEmail"] = "Email: admin@example.com",
                ["DemoPassword"] = "Contraseña: 1234",
                ["Home"] = "Inicio",
                ["Products"] = "Productos",
                ["Solutions"] = "Soluciones",
                ["Support"] = "Soporte",
                ["AboutUs"] = "Acerca",
                ["Contact"] = "Contactos",
                ["Login"] = "Iniciar sesión",
                ["Register"] = "Registrarse",
                ["AdminPanel"] = "Panel Admin",
                ["DoctorApplication"] = "Solicitud de Doctor",
                ["MyProfile"] = "Mi perfil",
                ["ApplyNow"] = "Solicitar ahora",
                ["ApproveMedicalDocuments"] = "Aprobar documentos médicos",
                ["CloseUserProfiles"] = "Cerrar perfiles",
                ["MakeRemoveAdmin"] = "Hacer/Quitar Admin",
                ["DoctorApplications"] = "Solicitudes de doctor",
                ["ShowMore"] = "Ver más",
                ["Approve"] = "Aprobar",
                ["Reject"] = "Rechazar",
                ["ViewFile"] = "Ver archivo",
                ["NotProvided"] = "No proporcionado",
                ["Logout"] = "Cerrar sesión",
                ["Submitted"] = "Enviado",
                ["AdminComment"] = "Comentario del administrador",
                ["PersonalInfo"] = "Información personal",
                ["ChangePassword"] = "Cambiar contraseña",
                ["ContactUs"] = "Contáctanos",
                ["ApplicationStatus"] = "Estado de la solicitud",
                ["SubmitApplication"] = "Enviar solicitud",
                ["FullName"] = "Nombre completo",
                ["Email"] = "Correo",
                ["Role"] = "Rol",
                ["NewPassword"] = "Nueva contraseña",
                ["EmploymentContract"] = "Contrato de trabajo",
                ["IDCard"] = "Documento de identidad",
                ["MedicalLicense"] = "Licencia médica",
                ["NoPending"] = "No hay elementos pendientes.",
                ["Terminate"] = "Terminar",
                ["MakeAdmin"] = "Hacer Admin",
                ["RemoveAdmin"] = "Quitar Admin",
                ["TellUsMore"] = "Cuéntanos más sobre ti",
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

    public string CurrentLanguageCode()
    {
        return CurrentLang();
    }

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

    public string T(string key)
    {
        var lang = CurrentLang();
        if (_data.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var val))
            return val;
        // fallback to English
        if (_data["en"].TryGetValue(key, out var ev)) return ev;
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
