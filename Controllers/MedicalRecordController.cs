using Microsoft.AspNetCore.Mvc;
using proekt.Models;
using proekt.Models.ViewModels;
using proekt.Services;

namespace proekt.Controllers;

public class MedicalRecordController : Controller
{
    private readonly MedicalRecordService _medRecordService;
    private readonly UserService _userService;

    public MedicalRecordController(MedicalRecordService medRecordService, UserService userService)
    {
        _medRecordService = medRecordService;
        _userService = userService;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private string? SessionRole => HttpContext.Session.GetString("UserRole");
    private int SessionUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
    private bool IsDoctor => SessionRole == UserRole.Doctor.ToString();
    private bool IsAdmin => SessionRole == UserRole.Admin.ToString() || SessionRole == "Manager";

    // ─── View Record (Patient / Doctor / Admin) ────────────────────────────

    [HttpGet]
    public IActionResult ViewRecord(int userId)
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("Login", "Home");

        var currentUserId = SessionUserId;
        var currentRole = SessionRole;

        // Patients may only see their own record
        if (!IsDoctor && !IsAdmin && currentUserId != userId)
            return RedirectToAction("AccessDenied");

        // Lazy-init the record if needed (e.g. seed users)
        var record = _medRecordService.GetRecordByUserId(userId)
                     ?? _medRecordService.CreateEmptyRecord(userId);

        var patient = _userService.GetUserById(userId);
        if (patient == null) return NotFound();

        var vm = new MedicalRecordViewModel
        {
            Patient = patient,
            Record = record,
            AuditLogs = _medRecordService.GetAuditLogs(record.Id),
            CanEdit = IsDoctor || IsAdmin
        };

        return View(vm);
    }

    // ─── Doctor Dashboard ──────────────────────────────────────────────────

    [HttpGet]
    public IActionResult DoctorDashboard(string? q)
    {
        if (!IsDoctor && !IsAdmin)
            return RedirectToAction("Index", "Home");

        var allUsers = _userService.GetAllUsers();

        List<User> patients;
        if (!string.IsNullOrWhiteSpace(q))
        {
            var ql = q.ToLower();
            patients = allUsers.Where(u =>
                (u.FullName?.ToLower().Contains(ql) ?? false) ||
                (u.Email?.ToLower().Contains(ql) ?? false) ||
                u.Id.ToString() == ql
            ).ToList();
        }
        else
        {
            patients = allUsers.ToList();
        }

        var vm = new DoctorDashboardViewModel
        {
            Patients = patients,
            SearchQuery = q
        };

        return View(vm);
    }

    // ─── Add Entry ─────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddEntry(int userId, string type, string title,
        string? description, string date)
    {
        if (!IsDoctor && !IsAdmin)
            return Unauthorized();

        if (!Enum.TryParse<MedicalEntryType>(type, out var entryType))
            entryType = MedicalEntryType.Note;

        var entryDate = DateTime.TryParse(date, out var d) ? d : DateTime.Today;
        var doctorId = SessionUserId;
        var doctorName = HttpContext.Session.GetString("UserName") ?? "Doctor";

        _medRecordService.AddEntry(userId, entryType, title, description, entryDate, doctorId, doctorName);

        TempData["RecordMessage"] = "Entry added successfully.";
        return RedirectToAction("ViewRecord", new { userId });
    }

    // ─── Edit Entry ────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditEntry(int entryId, int userId, string type, string title,
        string? description, string date)
    {
        if (!IsDoctor && !IsAdmin)
            return Unauthorized();

        if (!Enum.TryParse<MedicalEntryType>(type, out var entryType))
            entryType = MedicalEntryType.Note;

        var entryDate = DateTime.TryParse(date, out var d) ? d : DateTime.Today;
        var doctorId = SessionUserId;
        var doctorName = HttpContext.Session.GetString("UserName") ?? "Doctor";

        _medRecordService.EditEntry(entryId, entryType, title, description, entryDate, doctorId, doctorName);

        TempData["RecordMessage"] = "Entry updated successfully.";
        return RedirectToAction("ViewRecord", new { userId });
    }

    // ─── Update general record info ───────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateRecordInfo(int userId, string? bloodType, string? allergies,
        string? chronicConditions, string? generalNotes)
    {
        if (!IsDoctor && !IsAdmin)
            return Unauthorized();

        var doctorId = SessionUserId;
        var doctorName = HttpContext.Session.GetString("UserName") ?? "Doctor";

        _medRecordService.UpdateRecordInfo(userId, bloodType, allergies, chronicConditions, generalNotes, doctorId, doctorName);

        TempData["RecordMessage"] = "Record information updated.";
        return RedirectToAction("ViewRecord", new { userId });
    }

    // ─── Unauthorized fallback ─────────────────────────────────────────────

    public IActionResult AccessDenied()
    {
        return View("Unauthorized");
    }
}
