using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalMind.Web.Controllers;

[Authorize]
public class AppController : Controller
{
    public IActionResult Index() => View();
}
