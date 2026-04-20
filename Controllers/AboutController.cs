using Microsoft.AspNetCore.Mvc;

namespace FirstMvcApp.Controllers;

public class AboutController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
