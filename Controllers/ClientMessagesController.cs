using proekt.Models;
using proekt.Services;
using Microsoft.AspNetCore.Mvc;

namespace proekt.Controllers
{
    public class ClientMessagesController : Controller
    {
        private readonly ContactInquiryService _inquiryService;
        public ClientMessagesController(ContactInquiryService inquiryService)
        {
            _inquiryService = inquiryService;
        }

        private string SessionRole  => HttpContext.Session.GetString("UserRole") ?? "";
        private bool   IsAdminOrMgr => SessionRole == "Admin" || SessionRole == "Manager";

        public IActionResult Index()
        {
            if (!IsAdminOrMgr) return RedirectToAction("Index", "Home");
            var inquiries = _inquiryService.GetAllInquiries();
            return View(inquiries);
        }

        public IActionResult Details(int id)
        {
            if (!IsAdminOrMgr) return RedirectToAction("Index", "Home");
            var inquiry = _inquiryService.GetInquiryById(id);
            if (inquiry == null) return NotFound();
            return View(inquiry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Respond(int id, string response)
        {
            if (!IsAdminOrMgr) return Unauthorized();
            _inquiryService.RespondToInquiry(id, response);
            return RedirectToAction("Details", new { id });
        }
    }
}