using Microsoft.AspNetCore.Mvc;

namespace taChat.App.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}