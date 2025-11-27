using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using proekt.Models;
using proekt.Services;

namespace proekt.Controllers;

public class HomeController : Controller
{
    private readonly UserService _userService;

    public HomeController(UserService userService)
    {
        _userService = userService;
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
