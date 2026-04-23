using Microsoft.AspNetCore.Mvc;
using proekt.Models;
using proekt.Services;

namespace proekt.Controllers;

public class StatisticsController : Controller
{
    private readonly StatisticsService _statsService;
    private readonly DoctorProfileService _profileService;
    private readonly AppointmentService _apptService;
    private readonly UserService _userService;
    private readonly ActivityLogService _activityLog;

    public StatisticsController(StatisticsService statsService,
        DoctorProfileService profileService, AppointmentService apptService,
        UserService userService, ActivityLogService activityLog)
    {
        _statsService   = statsService;
        _profileService = profileService;
        _apptService    = apptService;
        _userService    = userService;
        _activityLog    = activityLog;
    }

    private string SessionRole  => HttpContext.Session.GetString("UserRole") ?? "";
    private bool   IsAdminOrMgr => SessionRole == "Admin" || SessionRole == "Manager";

    // ── Admin: Statistics Dashboard ───────────────────────────────────────────

    [HttpGet]
    public IActionResult Index()
    {
        if (!IsAdminOrMgr) return RedirectToAction("Index", "Home");

        var summary  = _statsService.GetPlatformSummary();
        var diagnoses = _statsService.GetMostCommonDiagnoses(10);
        var trend     = _statsService.GetVisitFrequency(30);
        var (bestDoc, bestProfile, bestCount) = _profileService.GetBestDoctorOfMonth(_userService, _apptService);

        ViewBag.Summary        = summary;
        ViewBag.Diagnoses      = diagnoses;
        ViewBag.VisitTrend     = trend;
        ViewBag.BestDoctor     = bestDoc;
        ViewBag.BestProfile    = bestProfile;
        ViewBag.BestCount      = bestCount;
        ViewBag.ActivityLogs   = _activityLog.GetRecent(50);

        return View();
    }

    // ── JSON endpoints for Chart.js ───────────────────────────────────────────

    [HttpGet]
    public IActionResult DiagnosisChart()
    {
        if (!IsAdminOrMgr) return Forbid();
        var data = _statsService.GetMostCommonDiagnoses(10);
        return Json(new
        {
            labels = data.Select(d => d.Diagnosis),
            values = data.Select(d => d.Count)
        });
    }

    [HttpGet]
    public IActionResult VisitTrend()
    {
        if (!IsAdminOrMgr) return Forbid();
        var data = _statsService.GetVisitFrequency(30);
        return Json(new
        {
            labels = data.Select(d => d.Date.ToString("MMM dd")),
            values = data.Select(d => d.Count)
        });
    }
}
