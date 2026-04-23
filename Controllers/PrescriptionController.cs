using Microsoft.AspNetCore.Mvc;
using proekt.Models;
using proekt.Services;

namespace proekt.Controllers;

public class PrescriptionController : Controller
{
    private readonly PrescriptionService _prescriptionService;
    private readonly UserService _userService;
    private readonly ActivityLogService _activityLog;

    public PrescriptionController(PrescriptionService prescriptionService,
        UserService userService, ActivityLogService activityLog)
    {
        _prescriptionService = prescriptionService;
        _userService         = userService;
        _activityLog         = activityLog;
    }

    private int    SessionUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
    private string SessionRole   => HttpContext.Session.GetString("UserRole") ?? "";
    private string SessionName   => HttpContext.Session.GetString("UserName") ?? "";
    private bool   IsLoggedIn    => HttpContext.Session.GetString("UserEmail") != null;
    private bool   IsDoctor      => SessionRole == "Doctor";
    private bool   IsAdminOrMgr  => SessionRole == "Admin" || SessionRole == "Manager";

    // ── Doctor: Create Prescription ───────────────────────────────────────────

    [HttpGet]
    public IActionResult Create(int patientId)
    {
        if (!IsDoctor && !IsAdminOrMgr) return RedirectToAction("Index", "Home");
        var patient = _userService.GetUserById(patientId);
        if (patient == null) return NotFound();
        ViewBag.Patient = patient;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(int patientId, string diagnosis, string medications,
        string? instructions, string? expiresAt)
    {
        if (!IsDoctor && !IsAdminOrMgr) return Unauthorized();

        DateTime? expDate = DateTime.TryParse(expiresAt, out var d) ? d : null;

        var rx = _prescriptionService.CreatePrescription(
            patientId, SessionUserId, SessionName,
            diagnosis, medications, instructions, expDate);

        _activityLog.Log(SessionUserId, SessionName, "CreatePrescription",
            $"Issued prescription #{rx.Id} for patient {patientId}: {diagnosis}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["RxSuccess"] = "Prescription issued successfully.";
        return RedirectToAction("DoctorPrescriptions");
    }

    // ── Doctor: My Issued Prescriptions ──────────────────────────────────────

    [HttpGet]
    public IActionResult DoctorPrescriptions()
    {
        if (!IsDoctor && !IsAdminOrMgr) return RedirectToAction("Index", "Home");

        var rxList = _prescriptionService.GetByDoctor(SessionUserId);
        var vm = rxList.Select(rx => new
        {
            Prescription = rx,
            Patient      = _userService.GetUserById(rx.PatientId)
        }).ToList();

        ViewBag.Items = vm;
        return View();
    }

    // ── Patient: My Prescriptions ─────────────────────────────────────────────

    [HttpGet]
    public IActionResult MyPrescriptions()
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Home");

        var rxList = _prescriptionService.GetByPatient(SessionUserId);
        ViewBag.Items = rxList;
        return View();
    }

    // ── Shared: Details ───────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Details(int id)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Home");
        var rx = _prescriptionService.GetById(id);
        if (rx == null) return NotFound();

        if (rx.PatientId != SessionUserId && rx.DoctorId != SessionUserId && !IsAdminOrMgr)
            return Unauthorized();

        ViewBag.Patient = _userService.GetUserById(rx.PatientId);
        ViewBag.Doctor  = _userService.GetUserById(rx.DoctorId);
        return View(rx);
    }
}
