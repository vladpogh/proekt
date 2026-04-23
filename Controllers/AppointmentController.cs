using Microsoft.AspNetCore.Mvc;
using proekt.Models;
using proekt.Services;

namespace proekt.Controllers;

public class AppointmentController : Controller
{
    private readonly AppointmentService _apptService;
    private readonly UserService _userService;
    private readonly DoctorProfileService _profileService;
    private readonly PaymentService _paymentService;
    private readonly ActivityLogService _activityLog;

    public AppointmentController(AppointmentService apptService, UserService userService,
        DoctorProfileService profileService, PaymentService paymentService,
        ActivityLogService activityLog)
    {
        _apptService   = apptService;
        _userService   = userService;
        _profileService = profileService;
        _paymentService = paymentService;
        _activityLog   = activityLog;
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private int    SessionUserId   => HttpContext.Session.GetInt32("UserId") ?? 0;
    private string SessionRole     => HttpContext.Session.GetString("UserRole") ?? "";
    private string SessionName     => HttpContext.Session.GetString("UserName") ?? "Unknown";
    private bool   IsLoggedIn      => HttpContext.Session.GetString("UserEmail") != null;
    private bool   IsDoctor        => SessionRole == "Doctor";
    private bool   IsAdminOrMgr    => SessionRole == "Admin" || SessionRole == "Manager";

    // ── Patient: Book ──────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Book(int doctorId)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Home");

        var doctor  = _userService.GetUserById(doctorId);
        if (doctor == null || doctor.Role != UserRole.Doctor) return NotFound();

        var profile = _profileService.GetOrCreate(doctorId);
        var today   = DateTime.Today;
        var slots   = _apptService.GetAvailableSlots(doctorId, today, _profileService);

        ViewBag.Doctor  = doctor;
        ViewBag.Profile = profile;
        ViewBag.Date    = today.ToString("yyyy-MM-dd");
        ViewBag.Slots   = slots;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Book(int doctorId, string date, string time, string? notes)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Home");

        if (!DateTime.TryParse($"{date} {time}", out var apptDateTime))
        {
            TempData["ApptError"] = "Invalid date or time.";
            return RedirectToAction("Book", new { doctorId });
        }

        var appt = _apptService.BookAppointment(SessionUserId, doctorId, apptDateTime, notes);

        _activityLog.Log(SessionUserId, SessionName, "BookAppointment",
            $"Booked appointment with Doctor ID {doctorId} on {apptDateTime:g}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["ApptSuccess"] = "Appointment booked successfully!";
        return RedirectToAction("MyAppointments");
    }

    // ── Patient: My Appointments ───────────────────────────────────────────────

    [HttpGet]
    public IActionResult MyAppointments()
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Home");
        var appts = _apptService.GetByPatient(SessionUserId);

        // Enrich with doctor info and payment status
        var vm = appts.Select(a => new
        {
            Appointment = a,
            Doctor      = _userService.GetUserById(a.DoctorId),
            Payment     = _paymentService.GetByAppointment(a.Id)
        }).ToList();

        ViewBag.Items = vm;
        return View();
    }

    // ── Doctor: Schedule ───────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult DoctorSchedule()
    {
        if (!IsDoctor && !IsAdminOrMgr) return RedirectToAction("Index", "Home");

        var appts = _apptService.GetByDoctor(SessionUserId);
        var vm = appts.Select(a => new
        {
            Appointment = a,
            Patient     = _userService.GetUserById(a.PatientId),
            Payment     = _paymentService.GetByAppointment(a.Id)
        }).ToList();

        ViewBag.Items = vm;
        return View();
    }

    // ── Doctor: Update Status ──────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateStatus(int id, string status, string? doctorNotes)
    {
        if (!IsDoctor && !IsAdminOrMgr) return Unauthorized();

        if (Enum.TryParse<AppointmentStatus>(status, out var s))
        {
            _apptService.UpdateStatus(id, s, doctorNotes);
            _activityLog.Log(SessionUserId, SessionName, "UpdateAppointmentStatus",
                $"Appointment {id} → {status}",
                HttpContext.Connection.RemoteIpAddress?.ToString());
        }

        return RedirectToAction("DoctorSchedule");
    }

    // ── Shared: Cancel ─────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        if (!IsLoggedIn) return Unauthorized();
        var appt = _apptService.GetById(id);
        if (appt == null) return NotFound();

        // Only the patient who booked it, or a doctor/admin, may cancel
        if (appt.PatientId != SessionUserId && !IsDoctor && !IsAdminOrMgr)
            return Unauthorized();

        _apptService.Cancel(id);
        _activityLog.Log(SessionUserId, SessionName, "CancelAppointment",
            $"Cancelled appointment {id}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["ApptSuccess"] = "Appointment cancelled.";
        return IsDoctor || IsAdminOrMgr
            ? RedirectToAction("DoctorSchedule")
            : RedirectToAction("MyAppointments");
    }

    // ── Shared: Details ────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Details(int id)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Home");
        var appt = _apptService.GetById(id);
        if (appt == null) return NotFound();

        if (appt.PatientId != SessionUserId && appt.DoctorId != SessionUserId && !IsAdminOrMgr)
            return Unauthorized();

        ViewBag.Doctor  = _userService.GetUserById(appt.DoctorId);
        ViewBag.Patient = _userService.GetUserById(appt.PatientId);
        ViewBag.Payment = _paymentService.GetByAppointment(appt.Id);
        return View(appt);
    }

    // ── Doctor: Fetch available slots (AJAX) ───────────────────────────────────

    [HttpGet]
    public IActionResult GetSlots(int doctorId, string date)
    {
        if (!DateTime.TryParse(date, out var d)) return BadRequest();
        var slots = _apptService.GetAvailableSlots(doctorId, d, _profileService);
        return Json(slots.Select(s => s.ToString("HH:mm")));
    }
}
