using Microsoft.AspNetCore.Mvc;
using proekt.Models;
using proekt.Services;

namespace proekt.Controllers;

public class PaymentController : Controller
{
    private readonly PaymentService _paymentService;
    private readonly AppointmentService _apptService;
    private readonly DoctorProfileService _profileService;
    private readonly UserService _userService;
    private readonly ActivityLogService _activityLog;

    public PaymentController(PaymentService paymentService, AppointmentService apptService,
        DoctorProfileService profileService, UserService userService, ActivityLogService activityLog)
    {
        _paymentService = paymentService;
        _apptService    = apptService;
        _profileService = profileService;
        _userService    = userService;
        _activityLog    = activityLog;
    }

    private int    SessionUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
    private string SessionRole   => HttpContext.Session.GetString("UserRole") ?? "";
    private string SessionName   => HttpContext.Session.GetString("UserName") ?? "";
    private bool   IsLoggedIn    => HttpContext.Session.GetString("UserEmail") != null;

    // ── Patient: Initiate Payment Page ────────────────────────────────────────

    [HttpGet]
    public IActionResult Initiate(int appointmentId)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Home");

        var appt = _apptService.GetById(appointmentId);
        if (appt == null || appt.PatientId != SessionUserId) return Forbid();

        var profile    = _profileService.GetByUserId(appt.DoctorId);
        var doctor     = _userService.GetUserById(appt.DoctorId);
        var amount     = profile?.ConsultationFee ?? 50m;

        // Create a pending payment record (idempotent)
        var payment = _paymentService.InitiatePayment(appointmentId, SessionUserId, appt.DoctorId, amount);

        ViewBag.Appointment = appt;
        ViewBag.Doctor      = doctor;
        ViewBag.Profile     = profile;
        return View(payment);
    }

    // ── Patient: Confirm Payment (simulate payment confirmation) ──────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Confirm(int paymentId)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Home");

        var payment = _paymentService.GetById(paymentId);
        if (payment == null || payment.PatientId != SessionUserId) return Forbid();

        _paymentService.ConfirmPayment(paymentId);

        _activityLog.Log(SessionUserId, SessionName, "PaymentConfirmed",
            $"Payment #{paymentId} confirmed. Amount: {payment.Amount} {payment.Currency}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["PaySuccess"] = $"Payment of {payment.Amount:F2} {payment.Currency} confirmed!";
        return RedirectToAction("Receipt", new { id = paymentId });
    }

    // ── Patient: Payment History ──────────────────────────────────────────────

    [HttpGet]
    public IActionResult History()
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Home");

        var payments = _paymentService.GetByPatient(SessionUserId);
        var vm = payments.Select(p => new
        {
            Payment     = p,
            Doctor      = _userService.GetUserById(p.DoctorId),
            Appointment = _apptService.GetById(p.AppointmentId)
        }).ToList();

        ViewBag.Items = vm;
        return View();
    }

    // ── Shared: Receipt ───────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Receipt(int id)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Home");

        var payment = _paymentService.GetById(id);
        if (payment == null) return NotFound();
        if (payment.PatientId != SessionUserId &&
            SessionRole != "Admin" && SessionRole != "Manager")
            return Forbid();

        ViewBag.Doctor      = _userService.GetUserById(payment.DoctorId);
        ViewBag.Patient     = _userService.GetUserById(payment.PatientId);
        ViewBag.Appointment = _apptService.GetById(payment.AppointmentId);
        return View(payment);
    }
}
