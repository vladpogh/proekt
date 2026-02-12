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
