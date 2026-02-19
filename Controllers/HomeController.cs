using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using proekt.Models;
using proekt.Services;

namespace proekt.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserService _userService;
        private readonly MedicalDocumentService _docService;
        private readonly DoctorApplicationService _appService;
        private readonly ContactInquiryService _inquiryService;

        [HttpGet]
        public IActionResult DoctorApplication()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.User.ToString())
                return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DoctorApplication(string about, IFormFile employmentContract, IFormFile idCard, IFormFile medicalLicense)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.User.ToString())
                return RedirectToAction("Index");

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            string? empPath = null;
            string? idPath = null;
            string? licPath = null;

            if (employmentContract != null && employmentContract.Length > 0)
            {
                var fname = $"emp_{userId}_{DateTime.Now.Ticks}_{Path.GetFileName(employmentContract.FileName)}";
                var full = Path.Combine(uploads, fname);
                using var stream = System.IO.File.Create(full);
                await employmentContract.CopyToAsync(stream);
                empPath = "/uploads/" + fname;
            }
            if (idCard != null && idCard.Length > 0)
            {
                var fname = $"id_{userId}_{DateTime.Now.Ticks}_{Path.GetFileName(idCard.FileName)}";
                var full = Path.Combine(uploads, fname);
                using var stream = System.IO.File.Create(full);
                await idCard.CopyToAsync(stream);
                idPath = "/uploads/" + fname;
            }
            if (medicalLicense != null && medicalLicense.Length > 0)
            {
                var fname = $"lic_{userId}_{DateTime.Now.Ticks}_{Path.GetFileName(medicalLicense.FileName)}";
                var full = Path.Combine(uploads, fname);
                using var stream = System.IO.File.Create(full);
                await medicalLicense.CopyToAsync(stream);
                licPath = "/uploads/" + fname;
            }

            var app = new DoctorApplication
            {
                UserId = userId,
                About = about,
                EmploymentContractPath = empPath,
                IdCardPath = idPath,
                MedicalLicensePath = licPath,
                Status = ApplicationStatus.Pending
            };
            _appService.Add(app);

            TempData["Message"] = "Your application has been submitted.";
            return RedirectToAction("Index");
        }

        public HomeController(UserService userService, MedicalDocumentService docService, DoctorApplicationService appService, ContactInquiryService inquiryService)
        {
            _userService = userService;
            _docService = docService;
            _appService = appService;
            _inquiryService = inquiryService;
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

        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login");

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var user = _userService.GetUserById(userId);
            ViewBag.User = user;

            var application = _appService.GetByUserId(userId);
            ViewBag.Application = application;

            // Pass all inquiries for this user to the view
            ViewBag.AllInquiries = _inquiryService.GetAllInquiries();

            return View();
        }

        [HttpPost]
        public IActionResult UpdateProfile(string fullName, string? phone, string? location)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0) return RedirectToAction("Login");

            _userService.UpdateUserProfile(userId, fullName, phone, location);
            
            // Update session if name changed
            if (!string.IsNullOrEmpty(fullName))
            {
                HttpContext.Session.SetString("UserName", fullName);
            }
            
            TempData["ProfileMessage"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var curId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (curId != userId) return Unauthorized();

            if (!_userService.VerifyPassword(userId, currentPassword))
            {
               TempData["ProfileMessage"] = "Current password is incorrect.";
               return RedirectToAction("Profile", new { tab = "password" });
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                TempData["ProfileMessage"] = "New password cannot be empty.";
                return RedirectToAction("Profile", new { tab = "password" });
            }

            var ok = _userService.ChangePassword(userId, newPassword);
            if (ok) TempData["ProfileMessage"] = "Password changed successfully.";
            else TempData["ProfileMessage"] = "Failed to change password.";
            return RedirectToAction("Profile", new { tab = "password" });
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

    public IActionResult DoctorApplicationDetails(int id)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != UserRole.Admin.ToString() && role != UserRole.Manager.ToString())
            return Unauthorized();
        var app = _appService.GetById(id);
        if (app == null) return NotFound();
        return View(app);
    }

    [HttpPost]
    public IActionResult ApproveDoctorApplication(int id, string? adminComment)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != UserRole.Admin.ToString() && role != UserRole.Manager.ToString())
            return Unauthorized();
        var app = _appService.GetById(id);
        if (app == null) return NotFound();
        _appService.UpdateStatus(id, ApplicationStatus.Approved, adminComment);
        // Make the user a Doctor
        _userService.UpdateUserRole(app.UserId, UserRole.Doctor);
        return RedirectToAction("AdminPanel");
    }

    [HttpPost]
    public IActionResult RejectDoctorApplication(int id, string? adminComment)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != UserRole.Admin.ToString() && role != UserRole.Manager.ToString())
            return Unauthorized();
        var app = _appService.GetById(id);
        if (app == null) return NotFound();
        _appService.UpdateStatus(id, ApplicationStatus.Rejected, adminComment);
        return RedirectToAction("AdminPanel");
    }

    [HttpGet]
    public IActionResult SetLanguage(string lang, string? returnUrl)
    {
        // save to session
        // use TranslationService via HttpContext.RequestServices
        var svc = HttpContext.RequestServices.GetService(typeof(TranslationService)) as TranslationService;
        svc?.SetLanguage(lang ?? "en");
        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index");
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Products()
    {
        return View();
    }


    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Contact(string Name, string Email, string Phone, string Subject, string Message)
    {
        var inquiry = new ContactInquiry
        {
            Name = Name,
            Email = Email,
            Phone = Phone,
            Subject = Subject,
            Message = Message
        };
        _inquiryService.AddInquiry(inquiry);
        TempData["Message"] = "Your inquiry has been sent.";
        return RedirectToAction("Contact");
    }

    public IActionResult Support()
    {
        return View();
    }

    public IActionResult FAQ()
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
                    HttpContext.Session.SetString("UserRole", user.Role.ToString());
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
