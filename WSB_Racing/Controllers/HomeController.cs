using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WSB_Racing.Models;

namespace WSB_Racing.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginModel model)
    {
        if (ModelState.IsValid)
        {
            // Überprüfen Sie die Anmeldedaten
            if (model.Email == "max.mustermann@wsb-sport.at" && model.Password == "1234")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ungültige Anmeldedaten.");
            }
        }
        return View(model);
    }

    public IActionResult Index()
    {
        return View();
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
