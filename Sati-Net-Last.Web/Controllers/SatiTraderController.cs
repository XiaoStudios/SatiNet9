using Microsoft.AspNetCore.Mvc;

namespace Sati_Net_Last.Web.Controllers;

public class SatiTraderController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}