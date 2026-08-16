using Microsoft.AspNetCore.Mvc;

namespace HealthWellnessMVC.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
