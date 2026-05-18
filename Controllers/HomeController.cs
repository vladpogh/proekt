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
        private readonly DoctorApplicationService _appService;
        private readonly ContactInquiryService _inquiryService;
        private readonly MedicalRecordService _medRecordService;
        private readonly TranslationService _loc;
        private readonly DoctorProfileService _profileService;
        private readonly ActivityLogService _activityLog;
        private readonly AppointmentService _apptService;
        private readonly PrescriptionService _prescriptionService;
        private readonly StatisticsService _statsService;

        [HttpGet]
        public IActionResult DoctorApplication()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.User.ToString())
                return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoctorApplication(string about, IFormFile employmentContract, IFormFile idCard, IFormFile medicalLicense)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.User.ToString())
                return RedirectToAction("Index");

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Strict Validation Rules
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var allowedMimeTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
            const long maxFileSize = 5 * 1024 * 1024; // 5 MB

            bool Validate(IFormFile? file, string fieldLabel, out string? error)
            {
                error = null;
                if (file == null || file.Length == 0) return true;

                if (file.Length > maxFileSize)
                {
                    error = $"{fieldLabel}: {_loc.T("FileTooLarge")}";
                    return false;
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    error = $"{fieldLabel}: {_loc.T("InvalidFileType")}";
                    return false;
                }

                var mime = file.ContentType.ToLowerInvariant();
                if (!allowedMimeTypes.Contains(mime))
                {
                    error = $"{fieldLabel}: {_loc.T("InvalidFileType")}";
                    return false;
                }

                return true;
            }

            if (!Validate(employmentContract, _loc.T("EmploymentContract"), out var err1))
                ModelState.AddModelError("employmentContract", err1!);
            if (!Validate(idCard, _loc.T("IDCard"), out var err2))
                ModelState.AddModelError("idCard", err2!);
            if (!Validate(medicalLicense, _loc.T("MedicalLicense"), out var err3))
                ModelState.AddModelError("medicalLicense", err3!);

            if (!ModelState.IsValid)
            {
                return View();
            }

            // Secure Storage Outside wwwroot
            var secureFolder = Path.Combine(Directory.GetCurrentDirectory(), "SecureUploads");
            if (!Directory.Exists(secureFolder)) Directory.CreateDirectory(secureFolder);

            string? empPath = null;
            string? idPath = null;
            string? licPath = null;

            async Task<string> SaveSecurely(IFormFile file, string prefix)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fname = $"{prefix}_{userId}_{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(secureFolder, fname);
                
                using var stream = System.IO.File.Create(fullPath);
                await file.CopyToAsync(stream);
                return "/secure-uploads/" + fname;
            }

            if (employmentContract != null && employmentContract.Length > 0)
            {
                empPath = await SaveSecurely(employmentContract, "emp");
            }
            if (idCard != null && idCard.Length > 0)
            {
                idPath = await SaveSecurely(idCard, "id");
            }
            if (medicalLicense != null && medicalLicense.Length > 0)
            {
                licPath = await SaveSecurely(medicalLicense, "lic");
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

            TempData["Message"] = _loc.T("AppSubmitted");
            return RedirectToAction("Index");
        }

        public HomeController(UserService userService, DoctorApplicationService appService,
            ContactInquiryService inquiryService, MedicalRecordService medRecordService,
            TranslationService loc, DoctorProfileService profileService,
            ActivityLogService activityLog, AppointmentService apptService,
            PrescriptionService prescriptionService, StatisticsService statsService)
        {
            _userService         = userService;
            _appService          = appService;
            _inquiryService      = inquiryService;
            _medRecordService    = medRecordService;
            _loc                 = loc;
            _profileService      = profileService;
            _activityLog         = activityLog;
            _apptService         = apptService;
            _prescriptionService = prescriptionService;
            _statsService        = statsService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TerminateUser(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.Admin.ToString() && role != UserRole.Manager.ToString())
                return Unauthorized();
            _userService.RemoveUser(id);
            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MakeAdmin(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != UserRole.Manager.ToString())
                return Unauthorized();
            _userService.UpdateUserRole(id, UserRole.Admin);
            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            ViewBag.ActivityLogs = _activityLog.GetRecent(50);
            ViewBag.PlatformSummary = _statsService.GetPlatformSummary();
            
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

            // Pass only inquiries for this user to the view
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                ViewBag.AllInquiries = _inquiryService.GetInquiriesByEmail(user.Email);
            }
            else
            {
                ViewBag.AllInquiries = new List<ContactInquiry>();
            }

            // Enriched data for Profile Dashboard
            ViewBag.RecentAppointments = _apptService.GetByPatient(userId).Take(5).ToList();
            ViewBag.RecentPrescriptions = _prescriptionService.GetByPatient(userId).Take(5).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(string fullName, string? phone, string? location)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0) return RedirectToAction("Login");

            _userService.UpdateUserProfile(userId, fullName, phone, location);
            if (!string.IsNullOrEmpty(fullName))
            {
                HttpContext.Session.SetString("UserName", fullName);
            }
            TempData["ProfileMessage"] = _loc.T("ProfileUpdated");
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var curId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (curId != userId) return Unauthorized();

            if (!_userService.VerifyPassword(userId, currentPassword))
            {
               TempData["ProfileMessage"] = _loc.T("CurPassIncorrect");
               return RedirectToAction("Profile", new { tab = "password" });
            }

            if (newPassword == "1234")
            {
                TempData["ProfileMessage"] = _loc.T("Pass1234NotAllowed");
                return RedirectToAction("Profile", new { tab = "password" });
            }

            var ok = _userService.ChangePassword(userId, newPassword);
            if (ok) TempData["ProfileMessage"] = _loc.T("PassChangedSuccess");
            else TempData["ProfileMessage"] = _loc.T("PassChangedFailed");
            return RedirectToAction("Profile", new { tab = "password" });
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
    [ValidateAntiForgeryToken]
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
    [ValidateAntiForgeryToken]
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
            // Fetch real stats from DB
            ViewBag.TotalRecords = _medRecordService.GetTotalRecordsCount();
            ViewBag.TotalDoctors = _userService.GetCountByRole(UserRole.Doctor);
            ViewBag.TotalUsers = _userService.GetAllUsers().Count; // Total registered users
            
            return View();
        }

    public IActionResult Products()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }


    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
        TempData["Message"] = _loc.T("InquirySent");
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
    [ValidateAntiForgeryToken]
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
                _activityLog.Log(user.Id, user.FullName ?? "User", "Login",
                    $"User logged in ({user.Role})",
                    HttpContext.Connection.RemoteIpAddress?.ToString());
            }
            return RedirectToAction("Index");
        }

        ModelState.AddModelError("", _loc.T("InvalidLogin"));
        return View(model);
    }

    public IActionResult Register()
    {
        if (HttpContext.Session.GetString("UserEmail") != null)
            return RedirectToAction("Index");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (model.Password == "1234")
        {
            ModelState.AddModelError("Password", _loc.T("Pass1234NotAllowed"));
        }

        if (ModelState.IsValid)
        {
            if (_userService.RegisterUser(model.FullName ?? "", model.Email ?? "", model.Password ?? ""))
            {
                HttpContext.Session.SetString("UserEmail", model.Email ?? "");
                HttpContext.Session.SetString("UserName", model.FullName ?? "");
                var user = _userService.GetUserByEmail(model.Email ?? "");
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserRole", user.Role.ToString());
                    _medRecordService.CreateEmptyRecord(user.Id);
                    _activityLog.Log(user.Id, user.FullName ?? "User", "Register",
                        "New user registered",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _loc.T("EmailExists"));
        }

        return View(model);
    }

    public IActionResult Logout()
    {
        var userId   = HttpContext.Session.GetInt32("UserId");
        var userName = HttpContext.Session.GetString("UserName") ?? "User";
        _activityLog.Log(userId, userName, "Logout", null,
            HttpContext.Connection.RemoteIpAddress?.ToString());
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    // ── Doctors list (patient-facing) ──────────────────────────────────────────
    public IActionResult Doctors()
    {
        var doctorList = _profileService.GetAllDoctors(_userService);
        ViewBag.Doctors = doctorList;
        return View();
    }

    // ── Doctor: Edit own profile ───────────────────────────────────────────────
    [HttpGet]
    public IActionResult DoctorProfileEdit()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Doctor") return RedirectToAction("Index");
        var userId  = HttpContext.Session.GetInt32("UserId") ?? 0;
        var profile = _profileService.GetOrCreate(userId);
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DoctorProfileEdit(string specialization, string? bio,
        decimal consultationFee, string availableFrom, string availableTo,
        string workingDays, int slotDuration)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Doctor") return Unauthorized();
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

        // Convert "HH:mm" to total minutes
        int Parse(string t) { var p = t.Split(':'); return int.Parse(p[0]) * 60 + int.Parse(p[1]); }

        _profileService.Update(userId, specialization, bio, consultationFee,
            Parse(availableFrom), Parse(availableTo), workingDays, slotDuration);

        TempData["ProfileMessage"] = _loc.T("ProfileUpdated");
        return RedirectToAction("Profile");
    }

    [HttpGet]
    public IActionResult DownloadDocument(int id, string type)
    {
        var role = HttpContext.Session.GetString("UserRole");
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (userId == 0) return RedirectToAction("Login", "Home");

        var app = _appService.GetById(id);
        if (app == null) return NotFound();

        // Security check: Only the applicant themselves or an Admin/Manager can view the uploaded documents
        if (app.UserId != userId && role != UserRole.Admin.ToString() && role != UserRole.Manager.ToString())
        {
            return Forbid();
        }

        string? relativePath = type switch
        {
            "contract" => app.EmploymentContractPath,
            "idcard" => app.IdCardPath,
            "license" => app.MedicalLicensePath,
            _ => null
        };

        if (string.IsNullOrEmpty(relativePath)) return NotFound();

        // Convert path to the secure uploads directory
        var secureFolder = Path.Combine(Directory.GetCurrentDirectory(), "SecureUploads");
        var fileName = Path.GetFileName(relativePath);
        var fullPath = Path.Combine(secureFolder, fileName);

        if (!System.IO.File.Exists(fullPath)) return NotFound();

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        return PhysicalFile(fullPath, contentType);
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
