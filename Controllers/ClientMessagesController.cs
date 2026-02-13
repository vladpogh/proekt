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

        public IActionResult Index()
        {
            var inquiries = _inquiryService.GetAllInquiries();
            return View(inquiries);
        }

        public IActionResult Details(int id)
        {
            var inquiry = _inquiryService.GetInquiryById(id);
            if (inquiry == null) return NotFound();
            return View(inquiry);
        }

        [HttpPost]
        public IActionResult Respond(int id, string response)
        {
            _inquiryService.RespondToInquiry(id, response);
            return RedirectToAction("Details", new { id });
        }
    }
}