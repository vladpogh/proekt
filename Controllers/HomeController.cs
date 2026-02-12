        [HttpGet]
        public IActionResult DoctorApplication()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.User.ToString())
                return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public IActionResult DoctorApplication(string about, IFormFile employmentContract, IFormFile idCard, IFormFile medicalLicense)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.User.ToString())
                return RedirectToAction("Index");
            // TODO: Save files and application data, notify admin/manager, etc.
            TempData["Message"] = "Your application has been submitted.";
            return RedirectToAction("Index");
        }
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using proekt.Models;
using proekt.Services;

namespace proekt.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserService _userService;
        private readonly MedicalDocumentService _docService;

        public HomeController(UserService userService, MedicalDocumentService docService)
        {
            _userService = userService;
            _docService = docService;
        }

        [HttpPost]
        public IActionResult TerminateUser(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.Admin.ToString() && role != UserRole.Manager.ToString())
                return Unauthorized();
            _userService.RemoveUser(id);
            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        public IActionResult MakeAdmin(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.Manager.ToString())
                return Unauthorized();
            _userService.UpdateUserRole(id, UserRole.Admin);
            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        public IActionResult RemoveAdmin(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.Manager.ToString())
                return Unauthorized();
            _userService.UpdateUserRole(id, UserRole.User);
            return RedirectToAction("AdminPanel");
        }

        public IActionResult AdminPanel()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.Admin.ToString() && role != UserRole.Manager.ToString())
            {
                return RedirectToAction("Index");
            }
            return View();
        }
    [HttpPost]
    public IActionResult ApproveDocument(int id, string? comment)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != UserRole.Admin.ToString() && role != UserRole.Manager.ToString())
            return Unauthorized();
        _docService.ApproveDocument(id, comment);
        return RedirectToAction("AdminPanel");
    }

    [HttpPost]
    public IActionResult RejectDocument(int id, string? comment)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != UserRole.Admin.ToString() && role != UserRole.Manager.ToString())
            return Unauthorized();
        _docService.RejectDocument(id, comment);
        return RedirectToAction("AdminPanel");
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Products()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Support()
    {
        return View();
    }

    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("UserEmail") != null)
            return RedirectToAction("Index");
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Password) && _userService.VerifyPassword(model.Email, model.Password))
        {
            HttpContext.Session.SetString("UserEmail", model.Email);
            var user = _userService.GetUserByEmail(model.Email);
            if (user != null)
            {
                HttpContext.Session.SetString("UserName", user.FullName ?? "User");
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
            }
            return RedirectToAction("Index");
        }

        ModelState.AddModelError("", "Invalid email or password");
        return View(model);
    }

    public IActionResult Register()
    {
        if (HttpContext.Session.GetString("UserEmail") != null)
            return RedirectToAction("Index");
        return View();
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!string.IsNullOrEmpty(model.FullName) && !string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Password) && _userService.RegisterUser(model.FullName, model.Email, model.Password))
        {
            HttpContext.Session.SetString("UserEmail", model.Email);
            HttpContext.Session.SetString("UserName", model.FullName);
            var user = _userService.GetUserByEmail(model.Email);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
            }
            return RedirectToAction("Index");
        }

        ModelState.AddModelError("", "Email already exists or invalid data");
        return View(model);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
}
