using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Authenticated users are sent straight to their dashboard;
        // anonymous visitors land on the login page.
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction("Login", "Account");
    }

    public IActionResult Error(int? statusCode = null)
    {
        ViewBag.StatusCode = statusCode ?? 500;
        return View();
    }
}
