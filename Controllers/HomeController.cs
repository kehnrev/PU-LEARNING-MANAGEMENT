using System.Diagnostics;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EduTrackAnalytics.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Features()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View(new ContactViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(ContactViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["Success"] = "Thanks for reaching out. The EduTrack support team will respond soon.";
        return RedirectToAction(nameof(Contact));
    }

    public IActionResult Offline()
    {
        return View();
    }

    public IActionResult Privacy() => RedirectToAction(nameof(About));

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = 500,
            Title = "Something went wrong",
            Message = "Something went wrong. Please go back to the dashboard or try again."
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HttpStatus(int code)
    {
        var title = code switch
        {
            400 => "Bad request",
            404 => "Page not found",
            _ => "Request error"
        };

        var message = code switch
        {
            400 => "The request could not be processed. Please refresh the page and try again.",
            404 => "The page you are looking for was not found.",
            _ => "Something went wrong. Please go back to the dashboard or try again."
        };

        return View("Error", new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = code,
            Title = title,
            Message = message
        });
    }
}
